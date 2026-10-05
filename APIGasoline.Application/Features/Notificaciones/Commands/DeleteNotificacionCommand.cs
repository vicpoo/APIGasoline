using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Domain.Interfaces;
using MediatR;

namespace APIGasoline.Application.Features.Notificaciones.Commands;

public record DeleteNotificacionCommand(int NotificacionId) : IRequest;

public class DeleteNotificacionCommandHandler : IRequestHandler<DeleteNotificacionCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteNotificacionCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(DeleteNotificacionCommand request, CancellationToken cancellationToken)
    {
        var notificacion = await _unitOfWork.Notificaciones.GetByIdAsync(request.NotificacionId, cancellationToken)
            ?? throw new NotFoundException($"No existe una notificación con el id {request.NotificacionId}.");

        _unitOfWork.Notificaciones.Remove(notificacion);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
