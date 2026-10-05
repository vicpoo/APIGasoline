using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Application.Features.Notificaciones.DTOs;
using APIGasoline.Domain.Interfaces;
using AutoMapper;
using MediatR;

namespace APIGasoline.Application.Features.Notificaciones.Commands;

public record MarcarNotificacionLeidaCommand(int NotificacionId) : IRequest<NotificacionDto>;

public class MarcarNotificacionLeidaCommandHandler : IRequestHandler<MarcarNotificacionLeidaCommand, NotificacionDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public MarcarNotificacionLeidaCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<NotificacionDto> Handle(MarcarNotificacionLeidaCommand request, CancellationToken cancellationToken)
    {
        var notificacion = await _unitOfWork.Notificaciones.GetByIdAsync(request.NotificacionId, cancellationToken)
            ?? throw new NotFoundException($"No existe una notificación con el id {request.NotificacionId}.");

        notificacion.MarcarLeida();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<NotificacionDto>(notificacion);
    }
}
