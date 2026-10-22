using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Application.Common.Interfaces;
using APIGasoline.Application.Features.SolicitudesReserva.DTOs;
using APIGasoline.Domain.Entities;
using APIGasoline.Domain.Enums;
using APIGasoline.Domain.Interfaces;
using AutoMapper;
using MediatR;
using APIGasoline.Application.Common.Outbox;

namespace APIGasoline.Application.Features.SolicitudesReserva.Commands;

/// <summary>
/// Aprueba la solicitud, marca el carro como ocupado y rechaza automáticamente
/// las demás solicitudes pendientes de ese mismo carro. Todo en un solo SaveChanges (atómico).
/// </summary>
public record AprobarSolicitudReservaCommand(Guid SolicitudReservaId, Guid EmpleadoConfirmadorId, string? Motivo) : IRequest<SolicitudReservaDto>;

public class AprobarSolicitudReservaCommandHandler : IRequestHandler<AprobarSolicitudReservaCommand, SolicitudReservaDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IDateTimeProvider _dateTime;
    private readonly NotificadorPush _notificador;

    public AprobarSolicitudReservaCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, IDateTimeProvider dateTime, NotificadorPush notificador)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _dateTime = dateTime;
        _notificador = notificador;
    }

    public async Task<SolicitudReservaDto> Handle(AprobarSolicitudReservaCommand request, CancellationToken cancellationToken)
    {
        var solicitud = await _unitOfWork.SolicitudesReserva.GetByIdAsync(request.SolicitudReservaId, cancellationToken)
            ?? throw new NotFoundException($"No existe una solicitud de reserva con el id {request.SolicitudReservaId}.");

        await SolicitudReservaValidator.ValidarConfirmadorAsync(_unitOfWork, solicitud.EmpresaId, request.EmpleadoConfirmadorId, cancellationToken);

        var carro = await _unitOfWork.Carros.GetByIdAsync(solicitud.CarroId, cancellationToken)
            ?? throw new NotFoundException($"No existe un carro con el id {solicitud.CarroId}.");

        // Un carro solo puede tener un usuario activo a la vez (HU-RES-01 / RES-02)
        if (!carro.EstaDisponible)
            throw new ConflictException($"El carro {carro.Placas} no está disponible (estado actual: {carro.Estado}).");

        if (await _unitOfWork.SolicitudesReserva.GetAprobadaByCarroAsync(carro.CarroId, cancellationToken) is not null)
            throw new ConflictException($"El carro {carro.Placas} ya está asignado a otra solicitud aprobada.");

        var ahora = _dateTime.Now;

        // Reglas de dominio: solo pendientes, no aprobar la propia, carro disponible
        solicitud.Aprobar(request.EmpleadoConfirmadorId, ahora, request.Motivo);
        // Ocupa el carro y deja el registro en la bitácora
        var cambio = CambioEstatusCarro.Registrar(
            carro,
            request.EmpleadoConfirmadorId,
            EstadoCarro.Ocupado,
            $"Asignado por la solicitud de reserva {solicitud.SolicitudReservaId}.",
            fechaCambio: ahora);

        await _unitOfWork.CambiosEstatusCarro.AddAsync(cambio, cancellationToken);

        // Cola de confirmación: las demás pendientes del mismo carro se rechazan
        var otrasPendientes = await _unitOfWork.SolicitudesReserva
            .GetPendientesByCarroAsync(solicitud.CarroId, solicitud.SolicitudReservaId, cancellationToken);

        foreach (var otra in otrasPendientes)
        {
            otra.Rechazar(
                request.EmpleadoConfirmadorId,
                ahora,
                $"Rechazada automáticamente: el carro fue asignado a la solicitud {solicitud.SolicitudReservaId}.");

            await _notificador.AgregarAsync(Notificacion.Crear(
                otra.EmpresaId,
                otra.EmpleadoSolicitanteId,
                TipoNotificacion.ResultadoSolicitud,
                otra.SolicitudReservaId,
                $"Tu solicitud del carro {carro.Placas} fue rechazada: el carro se asignó a otra solicitud."), cancellationToken);
        }

        await _notificador.AgregarAsync(Notificacion.Crear(
            solicitud.EmpresaId,
            solicitud.EmpleadoSolicitanteId,
            TipoNotificacion.ResultadoSolicitud,
            solicitud.SolicitudReservaId,
            $"Tu solicitud del carro {carro.Marca} {carro.Modelo} ({carro.Placas}) fue aprobada."), cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<SolicitudReservaDto>(solicitud);
    }
}
