using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Application.Common.Interfaces;
using APIGasoline.Application.Features.Auditoria.Common;
using APIGasoline.Application.Features.Auditoria.DTOs;
using APIGasoline.Domain.Enums;
using APIGasoline.Domain.Interfaces;
using AutoMapper;
using MediatR;

namespace APIGasoline.Application.Features.Auditoria.Commands;

/// <summary>
/// Registra el resultado de una revisión abierta: revisado, con observaciones o pendiente de información.
/// Si <see cref="Cerrar"/> es true, además la concluye. Cada cambio queda en auditoria_evento,
/// así se conserva el historial de la revisión.
/// </summary>
public record RegistrarResultadoRevisionCommand(
    Guid RevisionAuditoriaId,
    EstadoRevision EstadoRevision,
    string? Observaciones,
    bool Cerrar) : IRequest<RevisionAuditoriaDto>;

public class RegistrarResultadoRevisionCommandHandler : IRequestHandler<RegistrarResultadoRevisionCommand, RevisionAuditoriaDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IDateTimeProvider _dateTime;
    private readonly IUsuarioActual _usuarioActual;

    public RegistrarResultadoRevisionCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, IDateTimeProvider dateTime, IUsuarioActual usuarioActual)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _dateTime = dateTime;
        _usuarioActual = usuarioActual;
    }

    public async Task<RevisionAuditoriaDto> Handle(RegistrarResultadoRevisionCommand request, CancellationToken cancellationToken)
    {
        var revision = await _unitOfWork.RevisionesAuditoria.GetByIdAsync(request.RevisionAuditoriaId, cancellationToken)
            ?? throw new NotFoundException($"No existe una revisión de auditoría con el id {request.RevisionAuditoriaId}.");

        await PermisosAuditoria.ValidarEmpresaAsync(_usuarioActual, revision.EmpresaId, cancellationToken);

        if (_usuarioActual.EmpleadoId is null)
            throw new ForbiddenException("Solo un empleado puede registrar el resultado de una revisión.");

        // El dominio valida el estado y que la revisión siga abierta
        revision.Registrar(request.EstadoRevision, request.Observaciones);

        if (request.Cerrar)
            revision.Cerrar(_dateTime.Now);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<RevisionAuditoriaDto>(revision);
    }
}
