using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Application.Common.Interfaces;
using APIGasoline.Application.Features.Viajes.DTOs;
using APIGasoline.Domain.Interfaces;
using AutoMapper;
using MediatR;
using APIGasoline.Application.Features.RutasProcesadas.Notifications;

namespace APIGasoline.Application.Features.Viajes.Commands;

/// <summary>
/// Interrumpe el viaje. Si no se manda fecha de fin, se usa la hora actual.
/// </summary>
public record InterrumpirViajeCommand(
    Guid ViajeId,
    string Motivo,
    DateTime? FechaFin,
    decimal? UbicacionFinLat,
    decimal? UbicacionFinLng,
    decimal? KilometrajeFinal) : IRequest<ViajeDto>;

public class InterrumpirViajeCommandHandler : IRequestHandler<InterrumpirViajeCommand, ViajeDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IDateTimeProvider _dateTime;
    private readonly IPublisher _publisher;

    public InterrumpirViajeCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, IDateTimeProvider dateTime, IPublisher publisher)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _dateTime = dateTime;
        _publisher = publisher;
    }

    public async Task<ViajeDto> Handle(InterrumpirViajeCommand request, CancellationToken cancellationToken)
    {
        var viaje = await _unitOfWork.Viajes.GetByIdAsync(request.ViajeId, cancellationToken)
            ?? throw new NotFoundException($"No existe un viaje con el id {request.ViajeId}.");

        viaje.Interrumpir(
            request.Motivo,
            request.FechaFin ?? _dateTime.Now,
            request.UbicacionFinLat,
            request.UbicacionFinLng,
            request.KilometrajeFinal);

        if (request.KilometrajeFinal is not null)
        {
            var carro = await _unitOfWork.Carros.GetByIdAsync(viaje.CarroId, cancellationToken)
                ?? throw new NotFoundException($"No existe un carro con el id {viaje.CarroId}.");

            carro.AvanzarKilometraje(request.KilometrajeFinal.Value);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // El viaje ya quedó cerrado; procesar la ruta no puede deshacerlo
        await _publisher.Publish(new RutaViajeModificadaNotification(viaje.ViajeId), cancellationToken);

        return _mapper.Map<ViajeDto>(viaje);
    }
}
