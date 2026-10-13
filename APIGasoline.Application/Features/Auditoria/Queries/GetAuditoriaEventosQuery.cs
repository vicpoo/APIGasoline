using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Application.Common.Interfaces;
using APIGasoline.Application.Features.Auditoria.Common;
using APIGasoline.Application.Features.Auditoria.DTOs;
using APIGasoline.Domain.Enums;
using APIGasoline.Domain.Interfaces;
using MediatR;

namespace APIGasoline.Application.Features.Auditoria.Queries;

/// <summary>
/// Historial de cambios, del más reciente al más antiguo (máximo <see cref="Maximo"/> eventos).
/// Filtros: entidad (tabla) y su id, usuario, acción y rango de fechas.
/// </summary>
public record GetAuditoriaEventosQuery(
    Guid? EmpresaId,
    string? Entidad,
    Guid? EntidadId,
    Guid? UsuarioId,
    AccionAuditoria? Accion,
    DateTime? Desde,
    DateTime? Hasta,
    int Maximo = 500) : IRequest<IReadOnlyList<AuditoriaEventoDto>>;

public class GetAuditoriaEventosQueryHandler : IRequestHandler<GetAuditoriaEventosQuery, IReadOnlyList<AuditoriaEventoDto>>
{
    public const int MaximoPermitido = 2000;

    private readonly IUnitOfWork _unitOfWork;
    private readonly IUsuarioActual _usuarioActual;

    public GetAuditoriaEventosQueryHandler(IUnitOfWork unitOfWork, IUsuarioActual usuarioActual)
    {
        _unitOfWork = unitOfWork;
        _usuarioActual = usuarioActual;
    }

    public async Task<IReadOnlyList<AuditoriaEventoDto>> Handle(GetAuditoriaEventosQuery request, CancellationToken cancellationToken)
    {
        var empresaId = PermisosAuditoria.ResolverEmpresa(_usuarioActual, request.EmpresaId);

        if (request.Maximo is < 1 or > MaximoPermitido)
            throw new BadRequestException($"El máximo de eventos debe estar entre 1 y {MaximoPermitido}.");

        if (request.Desde is not null && request.Hasta is not null && request.Hasta < request.Desde)
            throw new BadRequestException("La fecha 'hasta' no puede ser anterior a 'desde'.");

        var eventos = await _unitOfWork.AuditoriaEventos.GetAllAsync(
            empresaId,
            request.Entidad,
            request.EntidadId,
            request.UsuarioId,
            request.Accion,
            request.Desde,
            request.Hasta,
            request.Maximo,
            cancellationToken);

        return eventos.Select(AuditoriaEventoDto.Desde).ToList();
    }
}
