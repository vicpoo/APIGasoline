using APIGasoline.Application.Features.Reportes.Common;
using APIGasoline.Application.Features.Reportes.DTOs;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.Rendering;
using static APIGasoline.Infrastructure.Reportes.TextosReporte;

namespace APIGasoline.Infrastructure.Reportes;

/// <summary>
/// Reporte en PDF con PDFsharp-MigraDoc (licencia MIT): encabezado, tarjetas de resumen y la tabla
/// por carro. Colores y tipografía de la app (azul #476D9E, títulos #080D38, Poppins).
/// </summary>
public sealed class ReportePdfGenerator : IReportePdfGenerator
{
    private static readonly Color Azul = Color.FromRgb(0x47, 0x6D, 0x9E);
    private static readonly Color Marino = Color.FromRgb(0x08, 0x0D, 0x38);
    private static readonly Color GrisFondo = Color.FromRgb(0xF2, 0xF2, 0xF2);
    private static readonly Color GrisBorde = Color.FromRgb(0xE0, 0xE0, 0xE0);
    private static readonly Color GrisTexto = Color.FromRgb(0x5F, 0x5F, 0x5F);

    public ReportePdfGenerator()
    {
        FuentesReporte.Registrar();
    }

    public byte[] Generar(ReporteFlotaDto reporte)
    {
        var documento = new Document();
        documento.Info.Title = Titulo(reporte);
        documento.Info.Author = "GasolineProject";

        var normal = documento.Styles[StyleNames.Normal]!;
        normal.Font.Name = FuentesReporte.Familia;
        normal.Font.Size = 8.5;
        normal.Font.Color = Color.FromRgb(0x1F, 0x1F, 0x1F);

        var seccion = documento.AddSection();
        seccion.PageSetup = documento.DefaultPageSetup.Clone();
        seccion.PageSetup.TopMargin = Unit.FromCentimeter(1.5);
        seccion.PageSetup.BottomMargin = Unit.FromCentimeter(1.5);
        seccion.PageSetup.LeftMargin = Unit.FromCentimeter(1.2);
        seccion.PageSetup.RightMargin = Unit.FromCentimeter(1.2);

        Encabezado(seccion, reporte);
        Resumen(seccion, reporte.Resumen);
        TablaPorCarro(seccion, reporte);
        Pie(seccion, reporte);

        var renderer = new PdfDocumentRenderer { Document = documento };
        renderer.RenderDocument();

        using var memoria = new MemoryStream();
        renderer.PdfDocument.Save(memoria, false);
        return memoria.ToArray();
    }

    private static void Encabezado(Section seccion, ReporteFlotaDto reporte)
    {
        var titulo = seccion.AddParagraph(Titulo(reporte));
        titulo.Format.Font.Size = 16;
        titulo.Format.Font.Bold = true;
        titulo.Format.Font.Color = Marino;
        titulo.Format.SpaceAfter = Unit.FromPoint(2);

        var empresa = seccion.AddParagraph(reporte.Empresa);
        empresa.Format.Font.Size = 10;
        empresa.Format.Font.Color = Azul;
        empresa.Format.Font.Bold = true;

        var periodo = seccion.AddParagraph($"{Periodo(reporte)}  ·  Generado el {FechaHora(reporte.GeneradoEn)}");
        periodo.Format.Font.Color = GrisTexto;
        periodo.Format.SpaceAfter = Unit.FromPoint(12);
    }

    /// <summary>Tarjetas: km recorridos, cargas, litros, gasto (y abajo rendimiento, costo por km, precio por litro).</summary>
    private static void Resumen(Section seccion, ResumenFlotaDto r)
    {
        var tarjetas = new (string Etiqueta, string Valor)[]
        {
            ("Km recorridos", Numero(r.KmRecorridos)),
            ("Viajes", Entero(r.Viajes)),
            ("Cargas de gasolina", Entero(r.Cargas)),
            ("Litros cargados", Numero(r.Litros)),
            ("Gasto total", Dinero(r.Gasto)),
            ("Rendimiento", Opcional(r.RendimientoKmPorLitro, v => $"{Numero(v)} km/L")),
            ("Costo por km", Opcional(r.CostoPorKm, Dinero)),
            ("Precio promedio", Opcional(r.PrecioPromedioLitro, v => $"{Dinero(v)} /L")),
        };

        var tabla = seccion.AddTable();
        tabla.Borders.Visible = false;
        const int columnas = 4;
        for (var i = 0; i < columnas; i++)
            tabla.AddColumn(Unit.FromCentimeter(4.65));

        for (var inicio = 0; inicio < tarjetas.Length; inicio += columnas)
        {
            var etiquetas = tabla.AddRow();
            var valores = tabla.AddRow();
            for (var i = 0; i < columnas; i++)
            {
                var (etiqueta, valor) = tarjetas[inicio + i];
                Tarjeta(etiquetas.Cells[i], valores.Cells[i], etiqueta, valor);
            }
            var separador = tabla.AddRow();
            separador.Height = Unit.FromPoint(6);
        }

        var nota = seccion.AddParagraph(
            $"{Entero(r.Carros)} vehículos en el reporte · {Entero(r.CarrosConActividad)} con movimiento en el periodo. " +
            "Km recorridos = km final − km inicial de los viajes cerrados (odómetro).");
        nota.Format.Font.Size = 7.5;
        nota.Format.Font.Color = GrisTexto;
        nota.Format.SpaceBefore = Unit.FromPoint(2);
        nota.Format.SpaceAfter = Unit.FromPoint(12);
    }

    private static void Tarjeta(Cell celdaEtiqueta, Cell celdaValor, string etiqueta, string valor)
    {
        foreach (var celda in new[] { celdaEtiqueta, celdaValor })
        {
            celda.Shading.Color = GrisFondo;
            celda.Borders.Left.Width = 2;
            celda.Borders.Left.Color = Azul;
        }

        var e = celdaEtiqueta.AddParagraph(etiqueta);
        e.Format.Font.Size = 7.5;
        e.Format.Font.Color = GrisTexto;
        e.Format.LeftIndent = Unit.FromPoint(6);
        e.Format.SpaceBefore = Unit.FromPoint(4);

        var v = celdaValor.AddParagraph(valor);
        v.Format.Font.Size = 12;
        v.Format.Font.Bold = true;
        v.Format.Font.Color = Marino;
        v.Format.LeftIndent = Unit.FromPoint(6);
        v.Format.SpaceAfter = Unit.FromPoint(4);
    }

    private static void TablaPorCarro(Section seccion, ReporteFlotaDto reporte)
    {
        var subtitulo = seccion.AddParagraph("Por vehículo (del que más gastó al que menos)");
        subtitulo.Format.Font.Size = 11;
        subtitulo.Format.Font.Bold = true;
        subtitulo.Format.Font.Color = Marino;
        subtitulo.Format.SpaceAfter = Unit.FromPoint(6);

        if (reporte.Carros.Count == 0)
        {
            seccion.AddParagraph("No hay vehículos para mostrar.").Format.Font.Color = GrisTexto;
            return;
        }

        // Ancho útil de la hoja carta/A4 con estos márgenes: ~18.6 cm
        var columnas = new (string Titulo, double Cm, bool Derecha)[]
        {
            ("Placas", 2.2, false),
            ("Vehículo", 3.6, false),
            ("Estado", 2.2, false),
            ("Viajes", 1.2, true),
            ("Km", 1.9, true),
            ("Cargas", 1.3, true),
            ("Litros", 1.7, true),
            ("Gasto", 2.1, true),
            ("km/L", 1.2, true),
            ("$/km", 1.2, true),
        };

        var tabla = seccion.AddTable();
        tabla.Borders.Color = GrisBorde;
        tabla.Borders.Width = 0.5;
        tabla.LeftPadding = Unit.FromPoint(3);
        tabla.RightPadding = Unit.FromPoint(3);
        tabla.TopPadding = Unit.FromPoint(2);
        tabla.BottomPadding = Unit.FromPoint(2);

        foreach (var (_, cm, derecha) in columnas)
        {
            var columna = tabla.AddColumn(Unit.FromCentimeter(cm));
            columna.Format.Alignment = derecha ? ParagraphAlignment.Right : ParagraphAlignment.Left;
        }

        // Encabezado (se repite en cada página)
        var encabezado = tabla.AddRow();
        encabezado.HeadingFormat = true;
        encabezado.Shading.Color = Azul;
        encabezado.Format.Font.Bold = true;
        encabezado.Format.Font.Color = Colors.White;
        for (var i = 0; i < columnas.Length; i++)
            encabezado.Cells[i].AddParagraph(columnas[i].Titulo);

        var par = false;
        foreach (var carro in reporte.Carros)
        {
            var fila = tabla.AddRow();
            if (par) fila.Shading.Color = GrisFondo;
            par = !par;

            var valores = new[]
            {
                carro.Placas,
                Vehiculo(carro),
                Estado(carro),
                Entero(carro.Viajes),
                Numero(carro.KmRecorridos),
                Entero(carro.Cargas),
                Numero(carro.Litros),
                Dinero(carro.Gasto),
                Opcional(carro.RendimientoKmPorLitro, Numero),
                Opcional(carro.CostoPorKm, Numero),
            };
            for (var i = 0; i < valores.Length; i++)
                fila.Cells[i].AddParagraph(valores[i]);
        }

        // Totales
        var r = reporte.Resumen;
        var total = tabla.AddRow();
        total.Format.Font.Bold = true;
        total.Shading.Color = Color.FromRgb(0xDD, 0xE5, 0xEF);
        var totales = new[]
        {
            "Total", $"{Entero(r.Carros)} vehículos", "",
            Entero(r.Viajes), Numero(r.KmRecorridos), Entero(r.Cargas), Numero(r.Litros), Dinero(r.Gasto),
            Opcional(r.RendimientoKmPorLitro, Numero), Opcional(r.CostoPorKm, Numero),
        };
        for (var i = 0; i < totales.Length; i++)
            total.Cells[i].AddParagraph(totales[i]);
    }

    private static void Pie(Section seccion, ReporteFlotaDto reporte)
    {
        var pie = seccion.Footers.Primary.AddParagraph();
        pie.Format.Alignment = ParagraphAlignment.Center;
        pie.Format.Font.Size = 7;
        pie.Format.Font.Color = GrisTexto;
        pie.AddText($"{reporte.Empresa} · {Periodo(reporte)} · Página ");
        pie.AddPageField();
        pie.AddText(" de ");
        pie.AddNumPagesField();
    }
}
