using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Application.Common.Interfaces;
using APIGasoline.Application.Common.Security;
using APIGasoline.Application.Features.Notificaciones.DTOs;
using APIGasoline.Domain.Interfaces;
using AutoMapper;
using APIGasoline.Application.Common.Messaging;

namespace APIGasoline.Application.Features.Notificaciones.Commands;

public sealed record MarcarNotificacionLeidaCommand(Guid NotificacionId) : ICommand<NotificacionDto>;

public sealed class MarcarNotificacionLeidaCommandHandler : ICommandHandler<MarcarNotificacionLeidaCommand, NotificacionDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IUsuarioActual _usuarioActual;

    public MarcarNotificacionLeidaCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, IUsuarioActual usuarioActual)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _usuarioActual = usuarioActual;
    }

    public async Task<NotificacionDto> Handle(MarcarNotificacionLeidaCommand request, CancellationToken cancellationToken)
    {
        var notificacion = await _unitOfWork.Notificaciones.GetByIdAsync(request.NotificacionId, cancellationToken)
            ?? throw new NotFoundException($"No existe una notificación con el id {request.NotificacionId}.");

        // Cada quien ve y maneja sus notificaciones; las de otro empleado, solo quien envía notificaciones
        await _usuarioActual.ValidarPropioOAlgunPrivilegioAsync(
            notificacion.EmpleadoDestinatarioId == _usuarioActual.EmpleadoId,
            [Permisos.NotificacionEnviar],
            "Solo puedes ver y manejar tus propias notificaciones",
            cancellationToken);

        notificacion.MarcarLeida();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<NotificacionDto>(notificacion);
    }
}
