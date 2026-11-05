using APIGasoline.Application.Features.Reportes.Common;
using APIGasoline.Application.Features.Reportes.DTOs;
using ClosedXML.Excel;
using static APIGasoline.Infrastructure.Reportes.TextosReporte;

namespace APIGasoline.Infrastructure.Reportes;

/// <summary>
/// Reporte en Excel (.xlsx) con ClosedXML (licencia MIT), TODO en una sola hoja ("Reporte"), de arriba abajo:
/// 1. Encabezado (título, empresa, periodo) y resumen.
/// 2. Por vehículo, con totales.
/// 3. Cargas de gasolina: cada carga con su centro de costos (con filtro, para ver un centro de costos).
/// Los números se guardan como números (no como texto) para que se puedan sumar, filtrar y graficar.
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
    private const string FormatoFechaHora = "dd/mm/yyyy hh:mm";

    /// <summary>Ancho de cada columna de la hoja (A..O): sirve para las tres secciones.</summary>
    private static readonly double[] Anchos = [26, 14, 30, 12, 16, 16, 12, 14, 12, 10, 12, 14, 14, 12, 13];

    public byte[] Generar(ReporteFlotaDto reporte)
    {
        using var libro = new XLWorkbook();
        var hoja = libro.Worksheets.Add("Reporte");

        var fila = Resumen(hoja, reporte);
        fila = PorVehiculo(hoja, reporte, fila + 2);
        Cargas(hoja, reporte, fila + 2);

        for (var c = 0; c < Anchos.Length; c++)
            hoja.Column(c + 1).Width = Anchos[c];

        using var memoria = new MemoryStream();
        libro.SaveAs(memoria);
        return memoria.ToArray();
    }

    /// <summary>Encabezado y resumen. Regresa la última fila usada.</summary>
    private static int Resumen(IXLWorksheet hoja, ReporteFlotaDto reporte)
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

        var fila = Seccion(hoja, 6, "Resumen");

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

        hoja.Cell(fila, 1).Value = "Km recorridos = km final − km inicial de los viajes cerrados. Las cargas se cuentan por la fecha de la carga.";
        hoja.Cell(fila, 1).Style.Font.Italic = true;
        hoja.Cell(fila, 1).Style.Font.FontColor = XLColor.Gray;
        return fila;
    }

    /// <summary>Tabla por vehículo con totales (fórmulas). Regresa la fila de totales.</summary>
    private static int PorVehiculo(IXLWorksheet hoja, ReporteFlotaDto reporte, int filaInicio)
    {
        var columnas = new (string Titulo, string? Formato)[]
        {
            ("Placas", null),
            ("Marca", null),
            ("Modelo", null),
            ("Año", "0"),
            ("Estado", null),
            ("Odómetro actual (km)", FormatoNumero),
            ("Viajes", FormatoEntero),
            ("Km recorridos", FormatoNumero),
            ("Km GPS", FormatoNumero),
            ("Cargas", FormatoEntero),
            ("Litros", FormatoNumero),
            ("Gasto", FormatoDinero),
            ("Rendimiento (km/L)", FormatoNumero),
            ("Costo por km", FormatoDinero),
            ("Último viaje", FormatoFecha),
        };

        var encabezado = Seccion(hoja, filaInicio, "Por vehículo (del que más gastó al que menos)");
        Encabezados(hoja, encabezado, columnas.Select(c => c.Titulo).ToArray());

        var primera = encabezado + 1;
        var fila = primera;
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
            Valores(hoja, fila, valores, columnas.Select(c => c.Formato).ToArray());
            fila++;
        }

        // Totales con fórmulas: si alguien corrige una fila en Excel, el total se actualiza
        Totales(hoja, fila, primera, fila - 1, columnas.Length, [7, 8, 9, 10, 11, 12], columnas.Select(c => c.Formato).ToArray());
        return fila;
    }

    /// <summary>Cada carga de gasolina con su centro de costos (con filtro para ver un solo centro).</summary>
    private static void Cargas(IXLWorksheet hoja, ReporteFlotaDto reporte, int filaInicio)
    {
        var cargas = reporte.DetalleCargas ?? [];
        var columnas = new (string Titulo, string? Formato)[]
        {
            ("Fecha", FormatoFechaHora),
            ("Placas", null),
            ("Vehículo", null),
            ("Litros", FormatoNumero),
            ("Monto", FormatoDinero),
            ("Precio por litro", FormatoDinero),
            ("Centro de costos", null),
            ("Ubicación", null),
        };
        const int columnaCentro = 7;
        const int columnaUbicacion = 8;

        var encabezado = Seccion(hoja, filaInicio, "Cargas de gasolina");
        if (cargas.Count == 0)
        {
            hoja.Cell(encabezado, 1).Value = "No hubo cargas de gasolina en el periodo.";
            hoja.Cell(encabezado, 1).Style.Font.FontColor = XLColor.Gray;
            return;
        }

        Encabezados(hoja, encabezado, columnas.Select(c => c.Titulo).ToArray());

        var primera = encabezado + 1;
        var fila = primera;
        foreach (var carga in cargas)
        {
            var valores = new XLCellValue[]
            {
                carga.FechaCarga,
                carga.Placas,
                carga.Vehiculo,
                (double)carga.Litros,
                carga.Monto is { } monto ? (double)monto : Blank.Value,
                PrecioLitro(carga) is { } precio ? (double)precio : Blank.Value,
                CentroCosto(carga),
                UrlMapa(carga) is null ? SinUbicacion : VerEnMapa,
            };
            Valores(hoja, fila, valores, columnas.Select(c => c.Formato).ToArray());
            if (carga.CentroCosto is null)
                hoja.Cell(fila, columnaCentro).Style.Font.FontColor = XLColor.Gray;

            // Ubicación: enlace que abre Google Maps con el punto donde se cargó
            var ubicacion = hoja.Cell(fila, columnaUbicacion);
            if (UrlMapa(carga) is { } url)
            {
                ubicacion.SetHyperlink(new XLHyperlink(new Uri(url), "Abrir en Google Maps"));
                ubicacion.Style.Font.FontColor = Azul;
                ubicacion.Style.Font.Underline = XLFontUnderlineValues.Single;
            }
            else
            {
                ubicacion.Style.Font.FontColor = XLColor.Gray;
            }
            fila++;
        }

        var ultima = fila - 1;
        // Filtro en la tabla de cargas: al filtrar un centro de costos, el total (SUBTOTAL) suma solo lo visible
        hoja.Range(encabezado, 1, ultima, columnas.Length).SetAutoFilter();
        Totales(hoja, fila, primera, ultima, columnas.Length, [4, 5], columnas.Select(c => c.Formato).ToArray());
        hoja.Cell(fila, 2).FormulaA1 = $"SUBTOTAL(3,B{primera}:B{ultima})";
        hoja.Cell(fila, 3).Value = "cargas";
    }

    // ---------------- Utilidades ----------------

    /// <summary>Título de sección; regresa la fila siguiente.</summary>
    private static int Seccion(IXLWorksheet hoja, int fila, string titulo)
    {
        hoja.Cell(fila, 1).Value = titulo;
        hoja.Cell(fila, 1).Style.Font.Bold = true;
        hoja.Cell(fila, 1).Style.Font.FontSize = 13;
        hoja.Cell(fila, 1).Style.Font.FontColor = Marino;
        return fila + 1;
    }

    private static void Encabezados(IXLWorksheet hoja, int fila, string[] titulos)
    {
        for (var c = 0; c < titulos.Length; c++)
        {
            var celda = hoja.Cell(fila, c + 1);
            celda.Value = titulos[c];
            celda.Style.Font.Bold = true;
            celda.Style.Font.FontColor = XLColor.White;
            celda.Style.Fill.BackgroundColor = Azul;
            celda.Style.Alignment.WrapText = true;
        }
    }

    private static void Valores(IXLWorksheet hoja, int fila, XLCellValue[] valores, string?[] formatos)
    {
        for (var c = 0; c < valores.Length; c++)
        {
            hoja.Cell(fila, c + 1).Value = valores[c];
            if (formatos[c] is { } formato)
                hoja.Cell(fila, c + 1).Style.NumberFormat.Format = formato;
        }
    }

    /// <summary>Fila "Total" con SUBTOTAL(9, ...) en las columnas indicadas (1 = A).</summary>
    private static void Totales(IXLWorksheet hoja, int fila, int primera, int ultima, int columnas, int[] sumar, string?[] formatos)
    {
        hoja.Cell(fila, 1).Value = "Total";
        if (ultima >= primera)
        {
            foreach (var c in sumar)
            {
                var letra = XLHelper.GetColumnLetterFromNumber(c);
                hoja.Cell(fila, c).FormulaA1 = $"SUBTOTAL(9,{letra}{primera}:{letra}{ultima})";
                if (formatos[c - 1] is { } formato)
                    hoja.Cell(fila, c).Style.NumberFormat.Format = formato;
            }
        }
        var rango = hoja.Range(fila, 1, fila, columnas);
        rango.Style.Font.Bold = true;
        rango.Style.Fill.BackgroundColor = AzulClaro;
    }
}
