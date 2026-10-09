using APIGasoline.Application.Features.Notificaciones.DTOs;
using APIGasoline.Domain.Enums;
using APIGasoline.Domain.Interfaces;
using AutoMapper;
using MediatR;

namespace APIGasoline.Application.Features.Notificaciones.Queries;

/// <summary>
/// Bandeja de la más reciente a la más antigua.
/// </summary>
public record GetNotificacionesQuery(
    Guid? EmpresaId = null,
    Guid? EmpleadoDestinatarioId = null,
    bool SoloNoLeidas = false,
    TipoNotificacion? Tipo = null) : IRequest<IReadOnlyList<NotificacionDto>>;

public class GetNotificacionesQueryHandler : IRequestHandler<GetNotificacionesQuery, IReadOnlyList<NotificacionDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetNotificacionesQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<NotificacionDto>> Handle(GetNotificacionesQuery request, CancellationToken cancellationToken)
    {
        var notificaciones = await _unitOfWork.Notificaciones.GetAllAsync(
            request.EmpresaId, request.EmpleadoDestinatarioId, request.SoloNoLeidas, request.Tipo, cancellationToken);

        return _mapper.Map<IReadOnlyList<NotificacionDto>>(notificaciones);
    }
}
