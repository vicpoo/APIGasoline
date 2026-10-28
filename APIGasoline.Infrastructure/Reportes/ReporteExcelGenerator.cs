using APIGasoline.Application.Features.Reportes.Common;
using APIGasoline.Application.Features.Reportes.DTOs;
using ClosedXML.Excel;
using static APIGasoline.Infrastructure.Reportes.TextosReporte;

namespace APIGasoline.Infrastructure.Reportes;

/// <summary>
/// Reporte en Excel (.xlsx) con ClosedXML (licencia MIT). Los números se guardan como números
/// (no como texto) para que se puedan sumar, filtrar y graficar en Excel.
/// Hojas: "Resumen" y "Por vehículo".
/// </summary>
public sealed class ReporteExcelGenerator : IReporteExcelGenerator
{
    private static readonly XLColor Azul = XLColor.FromHtml("#476D9E");
    private static readonly XLColor Marino = XLColor.FromHtml("#080D38");
    private static readonly XLColor GrisFondo = XLColor.FromHtml("#F2F2F2");
    private static readonly XLColor AzulClaro = XLColor.FromHtml("#DDE5EF");

    private const string FormatoNumero = "#,##0.00";
    private const string FormatoEntero = "#,##0";
    private const string FormatoDinero = "\"$\"#,##0.00";
    private const string FormatoFecha = "dd/mm/yyyy";

    public byte[] Generar(ReporteFlotaDto reporte)
    {
        using var libro = new XLWorkbook();
        HojaResumen(libro.Worksheets.Add("Resumen"), reporte);
        HojaPorVehiculo(libro.Worksheets.Add("Por vehículo"), reporte);

        using var memoria = new MemoryStream();
        libro.SaveAs(memoria);
        return memoria.ToArray();
    }

    private static void HojaResumen(IXLWorksheet hoja, ReporteFlotaDto reporte)
    {
        hoja.Cell(1, 1).Value = Titulo(reporte);
        hoja.Cell(1, 1).Style.Font.Bold = true;
        hoja.Cell(1, 1).Style.Font.FontSize = 16;
        hoja.Cell(1, 1).Style.Font.FontColor = Marino;

        hoja.Cell(2, 1).Value = reporte.Empresa;
        hoja.Cell(2, 1).Style.Font.Bold = true;
        hoja.Cell(2, 1).Style.Font.FontColor = Azul;
        hoja.Cell(3, 1).Value = Periodo(reporte);
        hoja.Cell(4, 1).Value = $"Generado el {FechaHora(reporte.GeneradoEn)}";

        var r = reporte.Resumen;
        var filas = new (string Etiqueta, double? Valor, string Formato)[]
        {
            ("Vehículos en el reporte", r.Carros, FormatoEntero),
            ("Vehículos con movimiento", r.CarrosConActividad, FormatoEntero),
            ("Viajes", r.Viajes, FormatoEntero),
            ("Km recorridos (odómetro)", (double)r.KmRecorridos, FormatoNumero),
            ("Km medidos por GPS", (double)r.KmGps, FormatoNumero),
            ("Cargas de gasolina", r.Cargas, FormatoEntero),
            ("Litros cargados", (double)r.Litros, FormatoNumero),
            ("Gasto total", (double)r.Gasto, FormatoDinero),
            ("Precio promedio por litro", (double?)r.PrecioPromedioLitro, FormatoDinero),
            ("Rendimiento (km/L)", (double?)r.RendimientoKmPorLitro, FormatoNumero),
            ("Costo por km", (double?)r.CostoPorKm, FormatoDinero),
        };

        var fila = 6;
        foreach (var (etiqueta, valor, formato) in filas)
        {
            hoja.Cell(fila, 1).Value = etiqueta;
            hoja.Cell(fila, 1).Style.Fill.BackgroundColor = GrisFondo;
            if (valor is { } v)
            {
                hoja.Cell(fila, 2).Value = v;
                hoja.Cell(fila, 2).Style.NumberFormat.Format = formato;
            }
            else
            {
                hoja.Cell(fila, 2).Value = "—";
            }
            hoja.Cell(fila, 2).Style.Font.Bold = true;
            fila++;
        }

        hoja.Cell(fila + 1, 1).Value = "Km recorridos = km final − km inicial de los viajes cerrados. Las cargas se cuentan por la fecha de la carga.";
        hoja.Cell(fila + 1, 1).Style.Font.Italic = true;
        hoja.Cell(fila + 1, 1).Style.Font.FontColor = XLColor.Gray;

        hoja.Column(1).Width = 30;
        hoja.Column(2).Width = 18;
    }

    private static void HojaPorVehiculo(IXLWorksheet hoja, ReporteFlotaDto reporte)
    {
        var columnas = new (string Titulo, double Ancho, string? Formato)[]
        {
            ("Placas", 12, null),
            ("Marca", 14, null),
            ("Modelo", 16, null),
            ("Año", 7, "0"),
            ("Estado", 16, null),
            ("Odómetro actual (km)", 16, FormatoNumero),
            ("Viajes", 9, FormatoEntero),
            ("Km recorridos", 14, FormatoNumero),
            ("Km GPS", 12, FormatoNumero),
            ("Cargas", 9, FormatoEntero),
            ("Litros", 12, FormatoNumero),
            ("Gasto", 14, FormatoDinero),
            ("Rendimiento (km/L)", 14, FormatoNumero),
            ("Costo por km", 12, FormatoDinero),
            ("Último viaje", 13, FormatoFecha),
        };

        for (var c = 0; c < columnas.Length; c++)
        {
            var celda = hoja.Cell(1, c + 1);
            celda.Value = columnas[c].Titulo;
            celda.Style.Font.Bold = true;
            celda.Style.Font.FontColor = XLColor.White;
            celda.Style.Fill.BackgroundColor = Azul;
            celda.Style.Alignment.WrapText = true;
            hoja.Column(c + 1).Width = columnas[c].Ancho;
        }

        var fila = 2;
        foreach (var carro in reporte.Carros)
        {
            var valores = new XLCellValue[]
            {
                carro.Placas,
                carro.Marca,
                carro.Modelo,
                carro.Anio is { } anio ? anio : Blank.Value,
                Estado(carro),
                (double)carro.KilometrajeActual,
                carro.Viajes,
                (double)carro.KmRecorridos,
                (double)carro.KmGps,
                carro.Cargas,
                (double)carro.Litros,
                (double)carro.Gasto,
                carro.RendimientoKmPorLitro is { } rendimiento ? (double)rendimiento : Blank.Value,
                carro.CostoPorKm is { } costo ? (double)costo : Blank.Value,
                carro.UltimoViaje is { } ultimo ? ultimo : Blank.Value,
            };
            for (var c = 0; c < valores.Length; c++)
            {
                hoja.Cell(fila, c + 1).Value = valores[c];
                if (columnas[c].Formato is { } formato)
                    hoja.Cell(fila, c + 1).Style.NumberFormat.Format = formato;
            }
            fila++;
        }

        // Totales con fórmulas: si alguien filtra o corrige una fila en Excel, el total se actualiza
        var ultimaFila = fila - 1;
        hoja.Cell(fila, 1).Value = "Total";
        if (ultimaFila >= 2)
        {
            foreach (var c in new[] { 7, 8, 9, 10, 11, 12 })
            {
                var letra = XLHelper.GetColumnLetterFromNumber(c);
                hoja.Cell(fila, c).FormulaA1 = $"SUBTOTAL(9,{letra}2:{letra}{ultimaFila})";
                hoja.Cell(fila, c).Style.NumberFormat.Format = columnas[c - 1].Formato;
            }
        }
        var totales = hoja.Range(fila, 1, fila, columnas.Length);
        totales.Style.Font.Bold = true;
        totales.Style.Fill.BackgroundColor = AzulClaro;

        if (ultimaFila >= 2)
            hoja.Range(1, 1, ultimaFila, columnas.Length).SetAutoFilter();
        hoja.SheetView.FreezeRows(1);
    }
}
