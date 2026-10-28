using APIGasoline.Application.Features.Reportes.DTOs;

namespace APIGasoline.Application.Features.Reportes.Common;

/// <summary>Convierte el reporte en PDF (implementación en Infrastructure con PDFsharp-MigraDoc).</summary>
public interface IReportePdfGenerator
{
    byte[] Generar(ReporteFlotaDto reporte);
}

/// <summary>Convierte el reporte en Excel .xlsx (implementación en Infrastructure con ClosedXML).</summary>
public interface IReporteExcelGenerator
{
    byte[] Generar(ReporteFlotaDto reporte);
}
