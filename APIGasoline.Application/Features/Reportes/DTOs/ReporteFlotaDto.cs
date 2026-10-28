using APIGasoline.Domain.Enums;

namespace APIGasoline.Application.Features.Reportes.DTOs;

/// <summary>
/// Reporte de la flota: por cada carro, cuántos km recorrió, cuántas cargas se le hicieron y
/// cuánto dinero se gastó, más los totales. Lo usa la app (vista previa) y los archivos PDF / Excel.
/// </summary>
public record ReporteFlotaDto(
    Guid? EmpresaId,
    string Empresa,
    /// <summary>null = toda la flota.</summary>
    Guid? CarroId,
    /// <summary>null = desde el primer registro.</summary>
    DateTime? Desde,
    /// <summary>null = hasta hoy.</summary>
    DateTime? Hasta,
    DateTime GeneradoEn,
    ResumenFlotaDto Resumen,
    /// <summary>Ordenados del que más gastó al que menos.</summary>
    IReadOnlyList<CarroReporteDto> Carros);

public record ResumenFlotaDto(
    int Carros,
    /// <summary>Carros con al menos un viaje o una carga en el periodo.</summary>
    int CarrosConActividad,
    int Viajes,
    /// <summary>Km por odómetro (km final - km inicial de los viajes cerrados).</summary>
    decimal KmRecorridos,
    /// <summary>Km medidos por GPS (rutas procesadas).</summary>
    decimal KmGps,
    int Cargas,
    decimal Litros,
    decimal Gasto,
    decimal? PrecioPromedioLitro,
    decimal? RendimientoKmPorLitro,
    decimal? CostoPorKm);

public record CarroReporteDto(
    Guid CarroId,
    string Placas,
    string Marca,
    string Modelo,
    int? Anio,
    EstadoCarro Estado,
    bool DadoDeBaja,
    /// <summary>Odómetro actual del carro (no depende del periodo).</summary>
    decimal KilometrajeActual,
    int Viajes,
    decimal KmRecorridos,
    decimal KmGps,
    int Cargas,
    decimal Litros,
    decimal Gasto,
    decimal? RendimientoKmPorLitro,
    decimal? CostoPorKm,
    DateTime? UltimoViaje);

public enum FormatoReporte
{
    Pdf = 0,
    Excel = 1
}

/// <summary>Archivo listo para descargar.</summary>
public record ArchivoReporte(byte[] Contenido, string TipoContenido, string NombreArchivo);
