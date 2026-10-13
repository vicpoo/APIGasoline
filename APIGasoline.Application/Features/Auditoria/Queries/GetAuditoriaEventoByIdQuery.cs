using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Application.Common.Interfaces;
using APIGasoline.Application.Features.Auditoria.Common;
using APIGasoline.Application.Features.Auditoria.DTOs;
using APIGasoline.Domain.Interfaces;
using MediatR;

namespace APIGasoline.Application.Features.Auditoria.Queries;

public record GetAuditoriaEventoByIdQuery(Guid AuditoriaEventoId) : IRequest<AuditoriaEventoDto>;

public class GetAuditoriaEventoByIdQueryHandler : IRequestHandler<GetAuditoriaEventoByIdQuery, AuditoriaEventoDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUsuarioActual _usuarioActual;

    public GetAuditoriaEventoByIdQueryHandler(IUnitOfWork unitOfWork, IUsuarioActual usuarioActual)
    {
        _unitOfWork = unitOfWork;
        _usuarioActual = usuarioActual;
    }

    public async Task<AuditoriaEventoDto> Handle(GetAuditoriaEventoByIdQuery request, CancellationToken cancellationToken)
    {
        var empresaPermitida = PermisosAuditoria.ResolverEmpresa(_usuarioActual, null);

        var evento = await _unitOfWork.AuditoriaEventos.GetByIdAsync(request.AuditoriaEventoId, cancellationToken);

        // Si es de otra empresa se responde igual que si no existiera
        if (evento is null || (empresaPermitida is not null && evento.EmpresaId != empresaPermitida))
            throw new NotFoundException($"No existe un evento de auditoría con el id {request.AuditoriaEventoId}.");

        return AuditoriaEventoDto.Desde(evento);
    }
}
