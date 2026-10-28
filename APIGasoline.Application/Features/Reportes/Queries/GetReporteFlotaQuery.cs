using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Application.Common.Interfaces;
using APIGasoline.Application.Features.Auditoria.Common;
using APIGasoline.Application.Features.Reportes.Common;
using APIGasoline.Application.Features.Reportes.DTOs;
using APIGasoline.Domain.Interfaces;
using MediatR;

namespace APIGasoline.Application.Features.Reportes.Queries;

/// <summary>
/// Reporte de la flota (o de un carro): km, cargas, litros y dinero por carro en el periodo.
/// Es parte del módulo de auditoría: usa el privilegio auditoria.ver y las mismas reglas de empresa
/// (solo la del token, salvo plataforma.multiempresa).
/// </summary>
public record GetReporteFlotaQuery(Guid? EmpresaId, Guid? CarroId, DateTime? Desde, DateTime? Hasta) : IRequest<ReporteFlotaDto>;

public class GetReporteFlotaQueryHandler : IRequestHandler<GetReporteFlotaQuery, ReporteFlotaDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUsuarioActual _usuarioActual;
    private readonly IDateTimeProvider _reloj;

    public GetReporteFlotaQueryHandler(IUnitOfWork unitOfWork, IUsuarioActual usuarioActual, IDateTimeProvider reloj)
    {
        _unitOfWork = unitOfWork;
        _usuarioActual = usuarioActual;
        _reloj = reloj;
    }

    public async Task<ReporteFlotaDto> Handle(GetReporteFlotaQuery request, CancellationToken cancellationToken)
    {
        var empresaId = await PermisosAuditoria.ResolverEmpresaAsync(_usuarioActual, request.EmpresaId, cancellationToken);

        if (request.Desde is not null && request.Hasta is not null && request.Hasta < request.Desde)
            throw new BadRequestException("La fecha 'hasta' no puede ser anterior a 'desde'.");

        var repositorio = _unitOfWork.ReportesFlota;

        var carros = await repositorio.GetCarrosAsync(empresaId, request.CarroId, cancellationToken);
        if (request.CarroId is not null && carros.Count == 0)
            throw new NotFoundException("No se encontró el vehículo en tu empresa.");

        var viajes = await repositorio.GetViajesPorCarroAsync(empresaId, request.CarroId, request.Desde, request.Hasta, cancellationToken);
        var cargas = await repositorio.GetCargasPorCarroAsync(empresaId, request.CarroId, request.Desde, request.Hasta, cancellationToken);
        var kmGps = await repositorio.GetKmGpsPorCarroAsync(empresaId, request.CarroId, request.Desde, request.Hasta, cancellationToken);

        var empresa = empresaId is null
            ? "Todas las empresas"
            : await repositorio.GetNombreEmpresaAsync(empresaId.Value, cancellationToken) ?? "Empresa";

        return ReporteFlotaCalculo.Armar(
            empresaId,
            empresa,
            request.CarroId,
            request.Desde,
            request.Hasta,
            _reloj.Now,
            carros,
            viajes,
            cargas,
            kmGps);
    }
}

/// <summary>El mismo reporte, como archivo PDF o Excel listo para descargar.</summary>
public record GetReporteFlotaArchivoQuery(Guid? EmpresaId, Guid? CarroId, DateTime? Desde, DateTime? Hasta, FormatoReporte Formato)
    : IRequest<ArchivoReporte>;

public class GetReporteFlotaArchivoQueryHandler : IRequestHandler<GetReporteFlotaArchivoQuery, ArchivoReporte>
{
    private readonly ISender _sender;
    private readonly IReportePdfGenerator _pdf;
    private readonly IReporteExcelGenerator _excel;

    public GetReporteFlotaArchivoQueryHandler(ISender sender, IReportePdfGenerator pdf, IReporteExcelGenerator excel)
    {
        _sender = sender;
        _pdf = pdf;
        _excel = excel;
    }

    public async Task<ArchivoReporte> Handle(GetReporteFlotaArchivoQuery request, CancellationToken cancellationToken)
    {
        var reporte = await _sender.Send(
            new GetReporteFlotaQuery(request.EmpresaId, request.CarroId, request.Desde, request.Hasta),
            cancellationToken);

        var nombre = NombreArchivo(reporte);
        return request.Formato switch
        {
            FormatoReporte.Excel => new ArchivoReporte(
                _excel.Generar(reporte),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                nombre + ".xlsx"),
            _ => new ArchivoReporte(_pdf.Generar(reporte), "application/pdf", nombre + ".pdf"),
        };
    }

    /// <summary>"reporte-flota_2026-10-01_2026-10-31" o "reporte-ABC-123-A_todo-el-historial".</summary>
    private static string NombreArchivo(ReporteFlotaDto reporte)
    {
        var alcance = reporte.CarroId is not null && reporte.Carros.Count == 1
            ? $"reporte-{reporte.Carros[0].Placas}"
            : "reporte-flota";
        var periodo = reporte.Desde is null && reporte.Hasta is null
            ? "todo-el-historial"
            : $"{reporte.Desde:yyyy-MM-dd}_{reporte.Hasta:yyyy-MM-dd}".Trim('_');

        var nombre = $"{alcance}_{periodo}";
        return new string(nombre.Select(ch => char.IsLetterOrDigit(ch) || ch is '-' or '_' ? ch : '-').ToArray());
    }
}
