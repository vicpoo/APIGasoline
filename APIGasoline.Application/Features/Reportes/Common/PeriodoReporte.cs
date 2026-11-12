using APIGasoline.Application.Common.Exceptions;

namespace APIGasoline.Application.Features.Reportes.Common;

/// <summary>
/// Acota los reportes de flota (F012): antes, sin fechas se calculaba TODO el historial (y de todas
/// las empresas con plataforma.multiempresa) en memoria, cada vez que alguien lo pedía.
/// - Sin fechas: el último mes.
/// - Solo "desde": hasta ahora. Solo "hasta": el mes anterior a esa fecha.
/// - Máximo 366 días (igual que la revisión de combustible).
/// - PDF y Excel con tope de filas en el detalle de cargas.
/// </summary>
public static class PeriodoReporte
{
    public static readonly TimeSpan PeriodoMaximo = TimeSpan.FromDays(366);

    /// <summary>Cargas como máximo en el detalle del PDF (más ya no se puede leer ni imprimir).</summary>
    public const int MaxFilasPdf = 5_000;

    /// <summary>Cargas como máximo en el detalle del Excel.</summary>
    public const int MaxFilasExcel = 50_000;

    public static (DateTime Desde, DateTime Hasta) Resolver(DateTime? desde, DateTime? hasta, DateTime ahora)
    {
        var fin = hasta ?? ahora;
        var inicio = desde ?? fin.AddMonths(-1);

        if (fin < inicio)
            throw new BadRequestException("La fecha 'hasta' no puede ser anterior a 'desde'.");

        if (fin - inicio > PeriodoMaximo)
            throw new BadRequestException($"El periodo del reporte no puede ser mayor a {PeriodoMaximo.TotalDays:0} días. Pide varios reportes más cortos.");

        return (inicio, fin);
    }

    public static void ValidarFilas(int filas, int maximo, string formato)
    {
        if (filas > maximo)
            throw new BadRequestException(
                $"El reporte tiene {filas} cargas y el {formato} admite como máximo {maximo}. Acorta el periodo o filtra por vehículo.");
    }
}
