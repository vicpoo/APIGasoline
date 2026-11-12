using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Application.Common.Interfaces;
using APIGasoline.Application.Common.Security;
using APIGasoline.Application.Features.SolicitudesReserva.DTOs;
using APIGasoline.Domain.Entities;
using APIGasoline.Domain.Enums;
using APIGasoline.Domain.Interfaces;
using AutoMapper;
using APIGasoline.Application.Common.Messaging;
using APIGasoline.Application.Common.Outbox;

namespace APIGasoline.Application.Features.SolicitudesReserva.Commands;

/// <summary>Rechaza la solicitud. El confirmador es el empleado del token (F008).</summary>
public sealed record RechazarSolicitudReservaCommand(Guid SolicitudReservaId, string Motivo) : ICommand<SolicitudReservaDto>;

public sealed class RechazarSolicitudReservaCommandHandler : ICommandHandler<RechazarSolicitudReservaCommand, SolicitudReservaDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IDateTimeProvider _dateTime;
    private readonly NotificadorPush _notificador;
    private readonly IUsuarioActual _usuarioActual;

    public RechazarSolicitudReservaCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, IDateTimeProvider dateTime, NotificadorPush notificador, IUsuarioActual usuarioActual)
    {
        _usuarioActual = usuarioActual;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _dateTime = dateTime;
        _notificador = notificador;
    }

    public async Task<SolicitudReservaDto> Handle(RechazarSolicitudReservaCommand request, CancellationToken cancellationToken)
    {
        var solicitud = await _unitOfWork.SolicitudesReserva.GetByIdAsync(request.SolicitudReservaId, cancellationToken)
            ?? throw new NotFoundException($"No existe una solicitud de reserva con el id {request.SolicitudReservaId}.");

        var confirmadorId = _usuarioActual.EmpleadoActor("rechazar solicitudes");
        await SolicitudReservaValidator.ValidarConfirmadorAsync(_unitOfWork, solicitud.EmpresaId, confirmadorId, cancellationToken);

        solicitud.Rechazar(confirmadorId, _dateTime.Now, request.Motivo);

        await _notificador.AgregarAsync(Notificacion.Crear(
            solicitud.EmpresaId,
            solicitud.EmpleadoSolicitanteId,
            TipoNotificacion.ResultadoSolicitud,
            solicitud.SolicitudReservaId,
            $"Tu solicitud de reserva fue rechazada. Motivo: {solicitud.MotivoResolucion}"), cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<SolicitudReservaDto>(solicitud);
    }
}
