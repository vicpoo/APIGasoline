using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Application.Common.Interfaces;
using APIGasoline.Application.Common.Security;
using APIGasoline.Domain.Interfaces;
using APIGasoline.Application.Common.Messaging;

namespace APIGasoline.Application.Features.Notificaciones.Commands;

public sealed record DeleteNotificacionCommand(Guid NotificacionId) : ICommand;

public sealed class DeleteNotificacionCommandHandler : ICommandHandler<DeleteNotificacionCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUsuarioActual _usuarioActual;

    public DeleteNotificacionCommandHandler(IUnitOfWork unitOfWork, IUsuarioActual usuarioActual)
    {
        _unitOfWork = unitOfWork;
        _usuarioActual = usuarioActual;
    }

    public async Task Handle(DeleteNotificacionCommand request, CancellationToken cancellationToken)
    {
        var notificacion = await _unitOfWork.Notificaciones.GetByIdAsync(request.NotificacionId, cancellationToken)
            ?? throw new NotFoundException($"No existe una notificación con el id {request.NotificacionId}.");

        // Cada quien ve y maneja sus notificaciones; las de otro empleado, solo quien envía notificaciones
        await _usuarioActual.ValidarPropioOAlgunPrivilegioAsync(
            notificacion.EmpleadoDestinatarioId == _usuarioActual.EmpleadoId,
            [Permisos.NotificacionEnviar],
            "Solo puedes ver y manejar tus propias notificaciones",
            cancellationToken);

        _unitOfWork.Notificaciones.Remove(notificacion);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
