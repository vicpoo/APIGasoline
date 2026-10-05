using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Application.Common.Interfaces;
using APIGasoline.Application.Features.SolicitudesReserva.DTOs;
using APIGasoline.Domain.Entities;
using APIGasoline.Domain.Enums;
using APIGasoline.Domain.Interfaces;
using AutoMapper;
using MediatR;

namespace APIGasoline.Application.Features.SolicitudesReserva.Commands;

/// <summary>
/// Cancela una solicitud pendiente o aprobada. Si estaba aprobada, libera el carro.
/// </summary>
public record CancelarSolicitudReservaCommand(int SolicitudReservaId, string? Motivo) : IRequest<SolicitudReservaDto>;

public class CancelarSolicitudReservaCommandHandler : IRequestHandler<CancelarSolicitudReservaCommand, SolicitudReservaDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IDateTimeProvider _dateTime;

    public CancelarSolicitudReservaCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, IDateTimeProvider dateTime)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _dateTime = dateTime;
    }

    public async Task<SolicitudReservaDto> Handle(CancelarSolicitudReservaCommand request, CancellationToken cancellationToken)
    {
        var solicitud = await _unitOfWork.SolicitudesReserva.GetByIdAsync(request.SolicitudReservaId, cancellationToken)
            ?? throw new NotFoundException($"No existe una solicitud de reserva con el id {request.SolicitudReservaId}.");

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
                    fechaCambio: ahora);

                await _unitOfWork.CambiosEstatusCarro.AddAsync(cambio, cancellationToken);
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<SolicitudReservaDto>(solicitud);
    }
}
