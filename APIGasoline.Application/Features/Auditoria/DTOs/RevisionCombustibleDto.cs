namespace APIGasoline.Application.Features.Auditoria.DTOs;

/// <summary>
/// Pantalla "Revisión de combustible": resumen del periodo y observaciones detectadas.
/// Una alerta NO significa fraude ni error: indica que alguien debería revisar el registro.
/// </summary>
public record RevisionCombustibleDto(
    Guid? EmpresaId,
    Guid? CarroId,
    DateTime Desde,
    DateTime Hasta,
    int ViajesRegistrados,
    decimal KilometrosOdometro,
    int CargasRegistradas,
    /// <summary>Consumo oficial del periodo: suma de litros cargados.</summary>
    decimal LitrosRegistrados,
    decimal MontoRegistrado,
    int CargasConComprobante,
    /// <summary>Registros distintos (viajes o cargas) con al menos una alerta.</summary>
    int RegistrosPorRevisar,
    IReadOnlyList<AlertaRevisionDto> Alertas);

public record AlertaRevisionDto(
    TipoAlertaRevision Tipo,
    string Descripcion,
    /// <summary>"viaje" o "carga_combustible".</summary>
    string Entidad,
    Guid EntidadId,
    Guid ViajeId,
    Guid CarroId,
    string? Placas,
    DateTime Fecha);

public enum TipoAlertaRevision
{
    /// <summary>La carga está en el servidor pero su comprobante todavía no.</summary>
    ComprobantePendiente = 0,

    /// <summary>Kilometraje final menor al inicial, o un viaje que no empieza donde terminó el anterior.</summary>
    KilometrajeInconsistente = 1,

    /// <summary>El odómetro declara bastantes más kilómetros que la ruta GPS.</summary>
    KilometrajeContraGps = 2,

    /// <summary>El registro se corrigió después de capturarse (detalle en auditoría).</summary>
    RegistroCorregido = 3,

    /// <summary>Un Confirmador cerró el viaje anticipadamente.</summary>
    ViajeInterrumpido = 4,

    /// <summary>Más litros de los que caben en el tanque.</summary>
    CargaInusual = 5,

    /// <summary>Dos cargas casi iguales en el mismo viaje.</summary>
    PosibleDuplicado = 6
}
