using System.Text.Encodings.Web;
using System.Text.Json;
using APIGasoline.Application.Common.Interfaces;
using APIGasoline.Domain.Entities;
using APIGasoline.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace APIGasoline.Infrastructure.Auditoria;

/// <summary>
/// Genera un <see cref="AuditoriaEvento"/> por cada registro creado, modificado o eliminado.
/// Los eventos se agregan al mismo SaveChanges, así que se guardan en la MISMA transacción
/// que el cambio: no puede existir un cambio sin su evento.
/// </summary>
public sealed class AuditoriaInterceptor : SaveChangesInterceptor
{
    // No se auditan: el propio historial, datos técnicos y tablas de alto volumen o calculadas
    private static readonly HashSet<Type> TiposExcluidos =
    [
        typeof(AuditoriaEvento),
        typeof(OutboxMensaje),
        typeof(Notificacion),
        typeof(DispositivoEmpleado),
        typeof(PuntoRuta),       // miles de puntos por viaje; nunca se modifican
        typeof(RutaProcesada)    // se recalcula a partir de los puntos
    ];

    // Nunca se guarda su valor en el historial
    private static readonly HashSet<string> ColumnasSensibles = new(StringComparer.OrdinalIgnoreCase)
    {
        "password_hash",
        "token_fcm"
    };

    private const string ValorOculto = "***";

    // Si cambian estas propiedades, el motivo del evento se toma de ellas
    private static readonly string[] PropiedadesMotivo = ["Motivo", "MotivoResolucion", "MotivoBaja", "MotivoInterrupcion"];

    private static readonly JsonSerializerOptions OpcionesJson = new()
    {
        // Conserva acentos y ñ legibles en el JSON
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    private readonly IUsuarioActual _usuarioActual;
    private readonly IContextoAuditoria _contexto;

    public AuditoriaInterceptor(IUsuarioActual usuarioActual, IContextoAuditoria contexto)
    {
        _usuarioActual = usuarioActual;
        _contexto = contexto;
    }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        AgregarEventos(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        AgregarEventos(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void AgregarEventos(DbContext? context)
    {
        if (context is null)
            return;

        context.ChangeTracker.DetectChanges();

        var entradas = context.ChangeTracker.Entries()
            .Where(e => e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .Where(e => !TiposExcluidos.Contains(e.Metadata.ClrType))
            .ToList();

        var eventos = new List<AuditoriaEvento>();

        foreach (var entrada in entradas)
        {
            var evento = CrearEvento(entrada);

            if (evento is not null)
                eventos.Add(evento);
        }

        if (eventos.Count > 0)
            context.Set<AuditoriaEvento>().AddRange(eventos);
    }

    private AuditoriaEvento? CrearEvento(EntityEntry entrada)
    {
        var llave = entrada.Metadata.FindPrimaryKey();

        if (llave is not { Properties.Count: 1 } || entrada.Property(llave.Properties[0].Name).CurrentValue is not Guid entidadId)
            return null;

        Dictionary<string, object?>? anteriores = null;
        Dictionary<string, object?>? nuevos = null;

        switch (entrada.State)
        {
            case EntityState.Added:
                nuevos = Instantanea(entrada, original: false);
                break;

            case EntityState.Deleted:
                anteriores = Instantanea(entrada, original: true);
                break;

            case EntityState.Modified:
                anteriores = new Dictionary<string, object?>();
                nuevos = new Dictionary<string, object?>();

                foreach (var propiedad in entrada.Properties.Where(p => p.IsModified))
                {
                    if (Equals(propiedad.OriginalValue, propiedad.CurrentValue))
                        continue;

                    var columna = NombreColumna(propiedad);
                    anteriores[columna] = Valor(propiedad, propiedad.OriginalValue, columna);
                    nuevos[columna] = Valor(propiedad, propiedad.CurrentValue, columna);
                }

                // Se marcó como modificado pero ningún valor cambió
                if (nuevos.Count == 0)
                    return null;

                break;
        }

        var accion = _contexto.ObtenerAccion(entidadId) ?? DeducirAccion(entrada);
        var motivo = _contexto.ObtenerMotivo(entidadId) ?? DeducirMotivo(entrada);

        return AuditoriaEvento.Registrar(
            ObtenerEmpresaId(entrada, entidadId),
            _usuarioActual.UsuarioId,
            accion,
            entrada.Metadata.GetTableName() ?? entrada.Metadata.ClrType.Name,
            entidadId,
            Serializar(anteriores),
            Serializar(nuevos),
            motivo,
            _usuarioActual.Origen);
    }

    private static AccionAuditoria DeducirAccion(EntityEntry entrada)
    {
        if (entrada.State == EntityState.Added)
            return AccionAuditoria.Crear;

        if (entrada.State == EntityState.Deleted)
            return AccionAuditoria.Eliminar;

        // Borrado lógico
        if (CambioA(entrada, "Eliminado", true))
            return AccionAuditoria.Eliminar;

        // Resolución de una solicitud de reserva
        if (entrada.Entity is SolicitudReserva solicitud && Cambio(entrada, nameof(SolicitudReserva.Estado)))
        {
            return solicitud.Estado switch
            {
                EstadoSolicitudReserva.Aprobada => AccionAuditoria.Aprobar,
                EstadoSolicitudReserva.Rechazada => AccionAuditoria.Rechazar,
                EstadoSolicitudReserva.Cancelada => AccionAuditoria.Cancelar,
                _ => AccionAuditoria.CambiarEstado
            };
        }

        // Cualquier otro cambio de estado (carro, viaje, revisión) o baja del carro
        if (Cambio(entrada, "Estado") || Cambio(entrada, "EstadoRevision") || Cambio(entrada, "DadoDeBaja") || Cambio(entrada, "Activo"))
            return AccionAuditoria.CambiarEstado;

        return AccionAuditoria.Actualizar;
    }

    private static string? DeducirMotivo(EntityEntry entrada)
    {
        foreach (var nombre in PropiedadesMotivo)
        {
            var propiedad = entrada.Metadata.FindProperty(nombre);

            if (propiedad is null)
                continue;

            var entradaPropiedad = entrada.Property(nombre);

            if ((entrada.State == EntityState.Added || entradaPropiedad.IsModified) && entradaPropiedad.CurrentValue is string { Length: > 0 } motivo)
                return motivo;
        }

        return null;
    }

    private static Guid? ObtenerEmpresaId(EntityEntry entrada, Guid entidadId)
    {
        if (entrada.Entity is Empresa)
            return entidadId;

        if (entrada.Metadata.FindProperty("EmpresaId") is null)
            return null;

        var propiedad = entrada.Property("EmpresaId");
        var valor = entrada.State == EntityState.Deleted ? propiedad.OriginalValue : propiedad.CurrentValue;

        return valor as Guid?;
    }

    private static bool Cambio(EntityEntry entrada, string nombre)
    {
        if (entrada.Metadata.FindProperty(nombre) is null)
            return false;

        var propiedad = entrada.Property(nombre);
        return propiedad.IsModified && !Equals(propiedad.OriginalValue, propiedad.CurrentValue);
    }

    private static bool CambioA(EntityEntry entrada, string nombre, object valor)
        => Cambio(entrada, nombre) && Equals(entrada.Property(nombre).CurrentValue, valor);

    private static Dictionary<string, object?> Instantanea(EntityEntry entrada, bool original)
    {
        var valores = new Dictionary<string, object?>();

        foreach (var propiedad in entrada.Properties)
        {
            var valor = original ? propiedad.OriginalValue : propiedad.CurrentValue;

            // Los valores que pone la BD (fechas por defecto) todavía no existen al insertar
            if (valor is null)
                continue;

            var columna = NombreColumna(propiedad);
            valores[columna] = Valor(propiedad, valor, columna);
        }

        return valores;
    }

    private static string NombreColumna(PropertyEntry propiedad)
        => propiedad.Metadata.GetColumnName();

    private static object? Valor(PropertyEntry propiedad, object? valor, string columna)
    {
        if (valor is null)
            return null;

        if (ColumnasSensibles.Contains(columna))
            return ValorOculto;

        // Enums: se guarda el mismo texto que en la columna enum(...) de MySQL ("en_mantenimiento")
        if (valor is Enum && propiedad.Metadata.FindTypeMapping()?.Converter is { } convertidor)
            return convertidor.ConvertToProvider(valor);

        return valor switch
        {
            Guid guid => guid.ToString(),
            Enum enumeracion => enumeracion.ToString(),
            _ => valor
        };
    }

    private static string? Serializar(Dictionary<string, object?>? valores)
        => valores is null || valores.Count == 0 ? null : JsonSerializer.Serialize(valores, OpcionesJson);
}
