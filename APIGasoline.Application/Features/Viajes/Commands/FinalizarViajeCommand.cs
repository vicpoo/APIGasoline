using APIGasoline.Application.Common.Interfaces;
using APIGasoline.Application.Common.Storage;
using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Application.Features.Viajes.DTOs;
using APIGasoline.Domain.Interfaces;
using AutoMapper;
using MediatR;
using APIGasoline.Application.Features.RutasProcesadas.Notifications;

namespace APIGasoline.Application.Features.Viajes.Commands;

/// <summary>
/// Finaliza el viaje, calcula la distancia y actualiza el kilometraje del carro.
/// </summary>
public record FinalizarViajeCommand(
    Guid ViajeId,
    DateTime FechaFin,
    decimal? UbicacionFinLat,
    decimal? UbicacionFinLng,
    decimal KilometrajeFinal) : IRequest<ViajeDto>;

public class FinalizarViajeCommandHandler : IRequestHandler<FinalizarViajeCommand, ViajeDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IFileStorageService _storage;
    private readonly IPublisher _publisher;

    public FinalizarViajeCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, IFileStorageService storage, IPublisher publisher)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _storage = storage;
        _publisher = publisher;
    }

    public async Task<ViajeDto> Handle(FinalizarViajeCommand request, CancellationToken cancellationToken)
    {
        var viaje = await _unitOfWork.Viajes.GetByIdAsync(request.ViajeId, cancellationToken)
            ?? throw new NotFoundException($"No existe un viaje con el id {request.ViajeId}.");

        var carro = await _unitOfWork.Carros.GetByIdAsync(viaje.CarroId, cancellationToken)
            ?? throw new NotFoundException($"No existe un carro con el id {viaje.CarroId}.");

        viaje.Finalizar(request.FechaFin, request.UbicacionFinLat, request.UbicacionFinLng, request.KilometrajeFinal);
        carro.AvanzarKilometraje(request.KilometrajeFinal);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // El viaje ya quedó cerrado; procesar la ruta no puede deshacerlo
        await _publisher.Publish(new RutaViajeModificadaNotification(viaje.ViajeId), cancellationToken);

        return await _mapper.Map<ViajeDto>(viaje).ConUrlsAsync(_storage, cancellationToken);
    }
}
