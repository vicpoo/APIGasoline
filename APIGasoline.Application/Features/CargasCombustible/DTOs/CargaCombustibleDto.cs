namespace APIGasoline.Application.Features.CargasCombustible.DTOs;

public record CargaCombustibleDto(
    Guid CargaCombustibleId,
    Guid EmpresaId,
    Guid ViajeId,
    decimal Litros,
    decimal? Monto,
    decimal? PrecioPorLitro,
    /// <summary>Llave del comprobante en el almacenamiento (lo que se guarda en BD).</summary>
    string FotoComprobante,
    /// <summary>URL temporal para ver el comprobante (la llena el handler, no AutoMapper).</summary>
    string? FotoComprobanteUrl,
    DateTime FechaCarga,
    decimal? UbicacionLat,
    decimal? UbicacionLng,
    /// <summary>Cuándo llegó el comprobante al almacenamiento. null = comprobante pendiente.</summary>
    DateTime? FechaSubidaComprobante,
    /// <summary>true si litros, monto o comprobante se corrigieron después de capturarse.</summary>
    bool Corregido,
    /// <summary>Hora del servidor en que llegó la carga.</summary>
    DateTime? FechaRecepcion);
