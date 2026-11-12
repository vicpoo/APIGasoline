using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Application.Common.Interfaces;
using APIGasoline.Application.Common.Security;
using APIGasoline.Application.Features.SolicitudesReserva.DTOs;
using APIGasoline.Domain.Entities;
using APIGasoline.Domain.Enums;
using APIGasoline.Domain.Interfaces;
using AutoMapper;
using APIGasoline.Application.Common.Messaging;

namespace APIGasoline.Application.Features.SolicitudesReserva.Commands;

/// <summary>
/// Cancela una solicitud pendiente o aprobada. Si estaba aprobada, libera el carro.
/// </summary>
public sealed record CancelarSolicitudReservaCommand(Guid SolicitudReservaId, string? Motivo) : ICommand<SolicitudReservaDto>;

public sealed class CancelarSolicitudReservaCommandHandler : ICommandHandler<CancelarSolicitudReservaCommand, SolicitudReservaDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IDateTimeProvider _dateTime;
    private readonly IUsuarioActual _usuarioActual;

    public CancelarSolicitudReservaCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, IDateTimeProvider dateTime, IUsuarioActual usuarioActual)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _dateTime = dateTime;
        _usuarioActual = usuarioActual;
    }

    public async Task<SolicitudReservaDto> Handle(CancelarSolicitudReservaCommand request, CancellationToken cancellationToken)
    {
        var solicitud = await _unitOfWork.SolicitudesReserva.GetByIdAsync(request.SolicitudReservaId, cancellationToken)
            ?? throw new NotFoundException($"No existe una solicitud de reserva con el id {request.SolicitudReservaId}.");

        // Cada quien cancela la suya; la de otro empleado, solo quien aprueba solicitudes
        await _usuarioActual.ValidarPropioOAlgunPrivilegioAsync(
            solicitud.EmpleadoSolicitanteId == _usuarioActual.EmpleadoId,
            [Permisos.SolicitudAprobar],
            "Solo puedes cancelar tus propias solicitudes",
            cancellationToken);

        if (await _unitOfWork.Viajes.ExisteEnCursoPorSolicitudAsync(solicitud.SolicitudReservaId, cancellationToken))
            throw new ConflictException("No se puede cancelar la reserva porque tiene un viaje en curso.");

        var estabaAprobada = solicitud.EstaAprobada;

        var ahora = _dateTime.Now;

        solicitud.Cancelar(ahora, request.Motivo);

        if (estabaAprobada)
        {
            var carro = await _unitOfWork.Carros.GetByIdAsync(solicitud.CarroId, cancellationToken)
                ?? throw new NotFoundException($"No existe un carro con el id {solicitud.CarroId}.");

            // Libera el carro y deja el registro en la bitácora (a nombre del solicitante)
            if (carro.Estado == EstadoCarro.Ocupado)
            {
                var cambio = CambioEstatusCarro.Registrar(
                    carro,
                    solicitud.EmpleadoSolicitanteId,
                    EstadoCarro.Disponible,
                    $"Liberado por cancelación de la solicitud de reserva {solicitud.SolicitudReservaId}.",
                    fechaCambio: ahora,
                    tipoMotivo: TipoMotivoCambio.Liberacion);

                await _unitOfWork.CambiosEstatusCarro.AddAsync(cambio, cancellationToken);
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<SolicitudReservaDto>(solicitud);
    }
}
