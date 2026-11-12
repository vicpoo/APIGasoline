using System.Reflection;
using APIGasoline.Application.Common.Interfaces;
using APIGasoline.Domain.Entities;
using APIGasoline.Domain.Enums;
using APIGasoline.Domain.Interfaces;

namespace APIGasoline.Application.Tests.Features.Auth;

/// <summary>Sesiones en memoria. Simula el concurrency token de token_hash con <see cref="FallarAlGuardar"/>.</summary>
internal sealed class SesionesEnMemoria : ISesionUsuarioRepository
{
    public List<SesionUsuario> Sesiones { get; } = [];

    public Task<SesionUsuario?> GetByIdAsync(Guid sesionUsuarioId, CancellationToken cancellationToken = default)
        => Task.FromResult(Sesiones.FirstOrDefault(s => s.SesionUsuarioId == sesionUsuarioId));

    public Task<SesionUsuario?> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default)
        => Task.FromResult(Sesiones.FirstOrDefault(s => s.TokenHash == tokenHash || s.TokenHashAnterior == tokenHash));

    public Task<IReadOnlyList<SesionUsuario>> GetActivasByUsuarioAsync(Guid usuarioId, DateTime ahora, CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<SesionUsuario>>(Sesiones
            .Where(s => s.UsuarioId == usuarioId && s.EstaActiva(ahora))
            .OrderByDescending(s => s.FechaUltimoUso)
            .ToList());

    public Task<IReadOnlyList<SesionUsuario>> GetActivasByEmpresaAsync(Guid empresaId, DateTime ahora, CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<SesionUsuario>>(Sesiones.Where(s => s.EmpresaId == empresaId && s.EstaActiva(ahora)).ToList());

    public Task AddAsync(SesionUsuario sesion, CancellationToken cancellationToken = default)
    {
        Sesiones.Add(sesion);
        return Task.CompletedTask;
    }

    public Task<int> EliminarObsoletasAsync(DateTime limite, CancellationToken cancellationToken = default)
        => Task.FromResult(Sesiones.RemoveAll(s => s.ExpiraEn < limite || s.FechaRevocacion < limite));
}

/// <summary>Excepción con el mismo nombre que la de EF, para simular el conflicto de concurrencia.</summary>
internal sealed class DbUpdateConcurrencyException : Exception;

/// <summary>
/// IUnitOfWork falso armado con DispatchProxy: solo responde lo que la prueba configura
/// (Sesiones, Usuarios.GetByIdAsync y lo que consulta PerfilUsuarioBuilder). Lo demás lanza.
/// </summary>
internal class UnidadFalsa : DispatchProxy
{
    public SesionesEnMemoria Sesiones { get; private set; } = new();
    public Dictionary<Guid, Usuario> Usuarios { get; } = [];

    /// <summary>Privilegios activos de cada rol (para el techo de privilegios).</summary>
    public Dictionary<Guid, string[]> ClavesPorRol { get; } = [];

    public int Guardados { get; private set; }

    /// <summary>Si se asigna, el siguiente SaveChanges lanza esta excepción (una sola vez).</summary>
    public Exception? FallarAlGuardar { get; set; }

    public static (IUnitOfWork UnitOfWork, UnidadFalsa Estado) Crear()
    {
        var proxy = Create<IUnitOfWork, UnidadFalsa>();
        return (proxy, (UnidadFalsa)(object)proxy);
    }

    protected override object? Invoke(MethodInfo? metodo, object?[]? args)
    {
        switch (metodo!.Name)
        {
            case "get_Sesiones":
                return Sesiones;
            case "get_Usuarios":
                return Repositorio<IUsuarioRepository>(nombre => nombre switch
                {
                    "GetByIdAsync" => a => Task.FromResult(Usuarios.GetValueOrDefault((Guid)a[0]!)),
                    "ExistsAsync" => a => Task.FromResult(Usuarios.ContainsKey((Guid)a[0]!)),
                    _ => null
                });
            case "get_Empleados":
                return Repositorio<IEmpleadoRepository>(nombre => nombre switch
                {
                    "GetByUsuarioIdAsync" => _ => Task.FromResult<Empleado?>(null),
                    "UsuarioAsignadoAsync" => _ => Task.FromResult(false),
                    _ => null
                });
            case "get_Roles":
                return Repositorio<IRolRepository>(nombre => nombre switch
                {
                    "GetActivosByUsuarioAsync" => _ => Task.FromResult<IReadOnlyList<Rol>>([]),
                    _ => null
                });
            case "get_Privilegios":
                return Repositorio<IPrivilegioRepository>(nombre => nombre switch
                {
                    "GetClavesActivasByRolesAsync" => a => Task.FromResult<IReadOnlyList<string>>(
                        ((IReadOnlyList<Guid>)a[0]!).SelectMany(id => ClavesPorRol.GetValueOrDefault(id) ?? []).Distinct().ToList()),
                    _ => null
                });
            case "SaveChangesAsync":
                if (FallarAlGuardar is { } ex)
                {
                    FallarAlGuardar = null;
                    return Task.FromException<int>(ex);
                }

                Guardados++;
                return Task.FromResult(1);
            default:
                throw new NotSupportedException($"La prueba no configuró IUnitOfWork.{metodo.Name}.");
        }
    }

    private static T Repositorio<T>(Func<string, Func<object?[], object?>?> respuestas) where T : class
    {
        var proxy = Create<T, RepositorioFalso>();
        ((RepositorioFalso)(object)proxy).Respuestas = respuestas;
        return proxy;
    }
}

internal class RepositorioFalso : DispatchProxy
{
    public Func<string, Func<object?[], object?>?> Respuestas { get; set; } = _ => null;

    protected override object? Invoke(MethodInfo? metodo, object?[]? args)
    {
        var respuesta = Respuestas(metodo!.Name)
            ?? throw new NotSupportedException($"La prueba no configuró {metodo.DeclaringType?.Name}.{metodo.Name}.");

        return respuesta(args ?? []);
    }
}

internal sealed class RelojFijo(DateTime ahora) : IDateTimeProvider
{
    public DateTime Now { get; set; } = ahora;
}

internal sealed class JwtFalso : IJwtTokenGenerator
{
    public TokenUsuario? Ultimo { get; private set; }

    public TokenGenerado Generar(TokenUsuario usuario)
    {
        Ultimo = usuario;
        return new TokenGenerado($"jwt-{usuario.SesionId}", new DateTime(2030, 1, 1, 0, 0, 0, DateTimeKind.Utc));
    }
}

internal sealed record UsuarioActualFalso(Guid? UsuarioId, Guid? SesionId, params string[] Privilegios) : IUsuarioActual
{
    public bool EstaAutenticado => UsuarioId is not null;
    public Guid? EmpleadoId { get; init; }
    public Guid? EmpresaId => null;
    public IReadOnlyCollection<string> Roles => [];
    public OrigenAuditoria Origen => OrigenAuditoria.Movil;

    public Task<IReadOnlySet<string>> GetPrivilegiosAsync(CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlySet<string>>(new HashSet<string>(Privilegios));

    public Task<bool> TienePrivilegioAsync(string clave, CancellationToken cancellationToken = default)
        => Task.FromResult(Privilegios.Contains(clave));
}
