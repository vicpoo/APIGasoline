using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Application.Common.Interfaces;
using APIGasoline.Application.Features.Auditoria.Common;
using APIGasoline.Application.Features.Auditoria.DTOs;
using MediatR;

namespace APIGasoline.Application.Features.Auditoria.Queries;

/// <summary>
/// "Revisión de combustible": reúne por empresa, carro y periodo los viajes, cargas y comprobantes,
/// y calcula las observaciones (comprobantes pendientes, kilometrajes inconsistentes, correcciones...).
/// Las alertas no se guardan; se calculan al consultar.
/// </summary>
public record GetRevisionCombustibleQuery(Guid? EmpresaId, Guid? CarroId, DateTime Desde, DateTime Hasta) : IRequest<RevisionCombustibleDto>;

public class GetRevisionCombustibleQueryHandler : IRequestHandler<GetRevisionCombustibleQuery, RevisionCombustibleDto>
{
    /// <summary>Para que la consulta no recorra años de datos en una sola llamada.</summary>
    public static readonly TimeSpan PeriodoMaximo = TimeSpan.FromDays(366);

    private readonly IConsultasRevisionCombustible _consultas;
    private readonly IUsuarioActual _usuarioActual;

    public GetRevisionCombustibleQueryHandler(IConsultasRevisionCombustible consultas, IUsuarioActual usuarioActual)
    {
        _consultas = consultas;
        _usuarioActual = usuarioActual;
    }

    public async Task<RevisionCombustibleDto> Handle(GetRevisionCombustibleQuery request, CancellationToken cancellationToken)
    {
        var empresaId = PermisosAuditoria.ResolverEmpresa(_usuarioActual, request.EmpresaId);

        if (request.Hasta < request.Desde)
            throw new BadRequestException("La fecha 'hasta' no puede ser anterior a 'desde'.");

        if (request.Hasta - request.Desde > PeriodoMaximo)
            throw new BadRequestException("El periodo de revisión no puede ser mayor a un año.");

        var viajes = await _consultas.GetViajesAsync(empresaId, request.CarroId, request.Desde, request.Hasta, cancellationToken);

        var viajeIds = viajes.Select(v => v.ViajeId).ToList();
        var carroIds = viajes.Select(v => v.CarroId).Distinct().ToList();

        var cargas = await _consultas.GetCargasAsync(viajeIds, cancellationToken);
        var carros = (await _consultas.GetCarrosAsync(carroIds, cancellationToken)).ToDictionary(c => c.CarroId);
        var kilometrajeAnterior = await _consultas.GetUltimoKilometrajePorCarroAsync(carroIds, request.Desde, cancellationToken);

        var alertas = DetectorAlertasRevision.Detectar(viajes, cargas, carros, kilometrajeAnterior);

        return new RevisionCombustibleDto(
            empresaId,
            request.CarroId,
            request.Desde,
            request.Hasta,
            ViajesRegistrados: viajes.Count,
            KilometrosOdometro: viajes.Sum(v => v.KilometrajeFinal is { } final ? final - v.KilometrajeInicial : 0m),
            CargasRegistradas: cargas.Count,
            LitrosRegistrados: cargas.Sum(c => c.Litros),
            MontoRegistrado: cargas.Sum(c => c.Monto ?? 0m),
            CargasConComprobante: cargas.Count(c => c.FechaSubidaComprobante is not null),
            RegistrosPorRevisar: alertas.Select(a => a.EntidadId).Distinct().Count(),
            Alertas: alertas);
    }
}
