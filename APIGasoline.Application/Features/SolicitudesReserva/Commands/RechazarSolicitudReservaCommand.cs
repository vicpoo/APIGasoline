using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Application.Common.Interfaces;
using APIGasoline.Application.Features.SolicitudesReserva.DTOs;
using APIGasoline.Domain.Entities;
using APIGasoline.Domain.Enums;
using APIGasoline.Domain.Interfaces;
using AutoMapper;
using MediatR;

namespace APIGasoline.Application.Features.SolicitudesReserva.Commands;

public record RechazarSolicitudReservaCommand(int SolicitudReservaId, int EmpleadoConfirmadorId, string Motivo) : IRequest<SolicitudReservaDto>;

public class RechazarSolicitudReservaCommandHandler : IRequestHandler<RechazarSolicitudReservaCommand, SolicitudReservaDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IDateTimeProvider _dateTime;

    public RechazarSolicitudReservaCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, IDateTimeProvider dateTime)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _dateTime = dateTime;
    }

    public async Task<SolicitudReservaDto> Handle(RechazarSolicitudReservaCommand request, CancellationToken cancellationToken)
    {
        var solicitud = await _unitOfWork.SolicitudesReserva.GetByIdAsync(request.SolicitudReservaId, cancellationToken)
            ?? throw new NotFoundException($"No existe una solicitud de reserva con el id {request.SolicitudReservaId}.");

        await SolicitudReservaValidator.ValidarConfirmadorAsync(_unitOfWork, solicitud.EmpresaId, request.EmpleadoConfirmadorId, cancellationToken);

        solicitud.Rechazar(request.EmpleadoConfirmadorId, _dateTime.Now, request.Motivo);

        await _unitOfWork.Notificaciones.AddAsync(Notificacion.Crear(
            solicitud.EmpresaId,
            solicitud.EmpleadoSolicitanteId,
            TipoNotificacion.ResultadoSolicitud,
            solicitud.SolicitudReservaId,
            $"Tu solicitud de reserva fue rechazada. Motivo: {solicitud.MotivoResolucion}"), cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<SolicitudReservaDto>(solicitud);
    }
}
