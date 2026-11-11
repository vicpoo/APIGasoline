using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Application.Common.Interfaces;
using APIGasoline.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace APIGasoline.Infrastructure.Persistence;

/// <summary>
/// Guardián de escrituras entre empresas: antes de cada SaveChanges revisa TODO lo que se va a
/// crear, modificar o borrar. Si un usuario de una empresa toca algo de otra, se cancela el
/// guardado completo con 403, aunque el handler haya olvidado validarlo.
///
/// Las lecturas las limita el filtro global de <see cref="AppDbContext"/>; esto cubre lo que el
/// filtro no ve: lo que se crea con el empresaId de otra empresa o colgado de un registro ajeno.
/// </summary>
public sealed class AislamientoEmpresaInterceptor : SaveChangesInterceptor
{
    /// <summary>
    /// Tablas técnicas que escribe el propio sistema (bitácora, outbox, sesiones). Los handlers que
    /// las tocan validan antes al usuario o al registro dueño.
    /// </summary>
    public static readonly IReadOnlySet<Type> Exentas = new HashSet<Type>
    {
        typeof(AuditoriaEvento),
        typeof(OutboxMensaje),
        typeof(SesionUsuario),
    };

    /// <summary>Catálogos de la plataforma: un usuario de una empresa nunca los modifica.</summary>
    public static readonly IReadOnlySet<Type> SoloPlataforma = new HashSet<Type>
    {
        typeof(Privilegio),
        typeof(Corporativo),
        typeof(Division),
    };

    /// <summary>Sin columna empresa_id: su empresa sale del usuario o del rol al que apuntan.</summary>
    public static readonly IReadOnlySet<Type> PorRelacion = new HashSet<Type>
    {
        typeof(UsuarioRol),
        typeof(RolPrivilegio),
    };

    private const string PropiedadEmpresa = "EmpresaId";

    private readonly IContextoEmpresa _contextoEmpresa;

    public AislamientoEmpresaInterceptor(IContextoEmpresa contextoEmpresa)
    {
        _contextoEmpresa = contextoEmpresa;
    }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        if (eventData.Context is { } context)
            ValidarAsync(context, sincrono: true, CancellationToken.None).GetAwaiter().GetResult();

        return base.SavingChanges(eventData, result);
    }

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is { } context)
            await ValidarAsync(context, sincrono: false, cancellationToken);

        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private async Task ValidarAsync(DbContext context, bool sincrono, CancellationToken cancellationToken)
    {
        var veTodas = _contextoEmpresa.VeTodasLasEmpresas;

        // 1) Nadie mueve registros de empresa. Se revisa ANTES de DetectChanges: EF no deja cambiar
        //    una propiedad que es parte de una llave (las FK compuestas empresa_id + id) y respondería 500.
        RevisarCambiosDeEmpresa(context, veTodas);

        // Plataforma y procesos del sistema pueden escribir en cualquier empresa
        if (veTodas)
            return;

        var empresa = _contextoEmpresa.EmpresaId;

        context.ChangeTracker.DetectChanges();

        var entradas = context.ChangeTracker.Entries()
            .Where(e => e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .ToList();

        foreach (var entrada in entradas)
        {
            var tipo = entrada.Metadata.ClrType;

            if (Exentas.Contains(tipo))
                continue;

            if (SoloPlataforma.Contains(tipo))
                throw new ForbiddenException("Solo la plataforma puede modificar este catálogo.");

            if (entrada.Entity is UsuarioRol)
            {
                foreach (var usuarioId in Valores<Guid>(entrada, nameof(UsuarioRol.UsuarioId)))
                    Exigir(await EmpresaDeUsuarioAsync(context, usuarioId, sincrono, cancellationToken), empresa);

                foreach (var rolId in Valores<Guid>(entrada, nameof(UsuarioRol.RolId)))
                    Exigir(await EmpresaDeRolAsync(context, rolId, sincrono, cancellationToken), empresa);

                continue;
            }

            if (entrada.Entity is RolPrivilegio)
            {
                foreach (var rolId in Valores<Guid>(entrada, nameof(RolPrivilegio.RolId)))
                    Exigir(await EmpresaDeRolAsync(context, rolId, sincrono, cancellationToken), empresa);

                continue;
            }

            if (entrada.Metadata.FindProperty(PropiedadEmpresa) is null)
            {
                // Falla cerrado: una tabla nueva debe decidir aquí cómo se aísla (ver ArquitecturaTests)
                throw new ForbiddenException($"No hay regla de aislamiento entre empresas para '{tipo.Name}'.");
            }

            foreach (var valor in Valores<Guid?>(entrada, PropiedadEmpresa))
                Exigir(valor, empresa);
        }
    }

    /// <summary>
    /// Con plataforma o sistema: solo se rechaza (409) si la empresa es parte de una llave.
    /// Con un usuario de empresa: cualquier cambio de empresa es 403.
    /// </summary>
    private static void RevisarCambiosDeEmpresa(DbContext context, bool veTodas)
    {
        var tracker = context.ChangeTracker;
        var detectar = tracker.AutoDetectChangesEnabled;
        tracker.AutoDetectChangesEnabled = false;

        try
        {
            foreach (var entrada in tracker.Entries())
            {
                if (entrada.State is not (EntityState.Unchanged or EntityState.Modified))
                    continue;

                var metadato = entrada.Metadata.FindProperty(PropiedadEmpresa);
                if (metadato is null)
                    continue;

                var propiedad = entrada.Property(PropiedadEmpresa);
                if (Equals(propiedad.CurrentValue, propiedad.OriginalValue))
                    continue;

                if (!veTodas)
                    throw new ForbiddenException("No puedes pasar registros a otra empresa.");

                if (metadato.IsKey())
                {
                    throw new ConflictException(
                        $"No se puede cambiar la empresa de un registro de '{entrada.Metadata.ClrType.Name}': " +
                        "sus datos relacionados son de la empresa original. Da de baja el registro y crea uno nuevo en la otra empresa.");
                }
            }
        }
        finally
        {
            tracker.AutoDetectChangesEnabled = detectar;
        }
    }

    /// <summary>Al crear, el valor nuevo; al modificar o borrar, el original y el actual.</summary>
    private static IEnumerable<T> Valores<T>(EntityEntry entrada, string propiedad)
    {
        var p = entrada.Property(propiedad);

        if (entrada.State != EntityState.Added)
            yield return (T)p.OriginalValue!;

        if (entrada.State != EntityState.Deleted)
            yield return (T)p.CurrentValue!;
    }

    private static void Exigir(Guid? empresaDelRegistro, Guid empresaDelUsuario)
    {
        if (empresaDelRegistro is null || empresaDelRegistro != empresaDelUsuario)
            throw new ForbiddenException("No puedes crear, modificar ni borrar información de otra empresa.");
    }

    private static async Task<Guid?> EmpresaDeUsuarioAsync(DbContext context, Guid usuarioId, bool sincrono, CancellationToken cancellationToken)
    {
        // Puede venir en el mismo SaveChanges (todavía no está en la BD)
        var local = context.ChangeTracker.Entries<Usuario>().FirstOrDefault(e => e.Entity.UsuarioId == usuarioId);
        if (local is not null)
            return local.Entity.EmpresaId;

        var consulta = context.Set<Usuario>().IgnoreQueryFilters().Where(u => u.UsuarioId == usuarioId).Select(u => u.EmpresaId);
        return sincrono ? consulta.FirstOrDefault() : await consulta.FirstOrDefaultAsync(cancellationToken);
    }

    /// <summary>Null también para los roles globales: un usuario de empresa no los asigna ni los cambia.</summary>
    private static async Task<Guid?> EmpresaDeRolAsync(DbContext context, Guid rolId, bool sincrono, CancellationToken cancellationToken)
    {
        var local = context.ChangeTracker.Entries<Rol>().FirstOrDefault(e => e.Entity.RolId == rolId);
        if (local is not null)
            return local.Entity.EmpresaId;

        var consulta = context.Set<Rol>().IgnoreQueryFilters().Where(r => r.RolId == rolId).Select(r => r.EmpresaId);
        return sincrono ? consulta.FirstOrDefault() : await consulta.FirstOrDefaultAsync(cancellationToken);
    }
}
