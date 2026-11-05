using APIGasoline.Domain.Enums;

namespace APIGasoline.Domain.ReadModels;

// Modelos de solo lectura (proyecciones) para el reporte de la flota.
// Las consultas ya llegan agrupadas por carro: la base de datos hace las sumas.

public record CarroReporte(
    Guid CarroId,
    string Placas,
    string Marca,
    string Modelo,
    int? Anio,
    EstadoCarro Estado,
    bool DadoDeBaja,
    decimal KilometrajeActual);

/// <summary>Viajes del carro en el periodo y los km por odómetro (solo viajes con km final válido).</summary>
public record ViajesPorCarro(Guid CarroId, int Viajes, decimal KmRecorridos, DateTime? UltimoViaje);

/// <summary>Cargas de gasolina del carro en el periodo (por fecha de la carga).</summary>
public record CargasPorCarro(Guid CarroId, int Cargas, decimal Litros, decimal Monto);

/// <summary>
/// Una carga de gasolina del periodo, para el detalle del PDF / Excel. El centro de costos es el de la
/// carga o, si la carga no tiene, el del viaje.
/// </summary>
public record CargaDetalle(
    Guid CargaCombustibleId,
    DateTime FechaCarga,
    Guid CarroId,
    decimal Litros,
    decimal? Monto,
    string? CentroCostoClave,
    string? CentroCostoNombre,
    /// <summary>Dónde se hizo la carga (null = sin ubicación).</summary>
    decimal? Latitud = null,
    decimal? Longitud = null);

/// <summary>Km medidos por GPS (rutas procesadas) de los viajes del carro en el periodo.</summary>
public record KmGpsPorCarro(Guid CarroId, decimal KmGps);
