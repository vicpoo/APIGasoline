namespace APIGasoline.Application.Features.CargasCombustible.DTOs;

public record CargaCombustibleDto(
    int CargaCombustibleId,
    int EmpresaId,
    int ViajeId,
    decimal Litros,
    decimal? Monto,
    decimal? PrecioPorLitro,
    string FotoComprobante,
    DateTime FechaCarga,
    decimal? UbicacionLat,
    decimal? UbicacionLng,
    bool Sincronizado);
