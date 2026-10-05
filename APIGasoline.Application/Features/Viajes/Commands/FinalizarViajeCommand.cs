using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Application.Features.Viajes.DTOs;
using APIGasoline.Domain.Interfaces;
using AutoMapper;
using MediatR;

namespace APIGasoline.Application.Features.Viajes.Commands;

/// <summary>
/// Finaliza el viaje, calcula la distancia y actualiza el kilometraje del carro.
/// </summary>
public record FinalizarViajeCommand(
    int ViajeId,
    DateTime FechaFin,
    decimal? UbicacionFinLat,
    decimal? UbicacionFinLng,
    decimal KilometrajeFinal) : IRequest<ViajeDto>;

public class FinalizarViajeCommandHandler : IRequestHandler<FinalizarViajeCommand, ViajeDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public FinalizarViajeCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
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

        return _mapper.Map<ViajeDto>(viaje);
    }
}
