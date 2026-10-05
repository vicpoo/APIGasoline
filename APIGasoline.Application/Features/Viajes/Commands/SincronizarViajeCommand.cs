using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Application.Features.Viajes.Common;
using APIGasoline.Application.Features.Viajes.DTOs;
using APIGasoline.Domain.Entities;
using APIGasoline.Domain.Enums;
using APIGasoline.Domain.Interfaces;
using AutoMapper;
using MediatR;

namespace APIGasoline.Application.Features.Viajes.Commands;

/// <summary>
/// Sube un viaje que la app registró sin conexión (offline-first).
/// Puede venir en curso, finalizado o interrumpido. Queda con sincronizado = true.
/// </summary>
public record SincronizarViajeCommand(
    int EmpresaId,
    int CarroId,
    int? SolicitudReservaId,
    int EmpleadoOperadorId,
    DateTime FechaInicio,
    decimal? UbicacionInicioLat,
    decimal? UbicacionInicioLng,
    decimal KilometrajeInicial,
    EstadoViaje Estado,
    DateTime? FechaFin,
    decimal? UbicacionFinLat,
    decimal? UbicacionFinLng,
    decimal? KilometrajeFinal,
    string? MotivoInterrupcion) : IRequest<ViajeDto>;

public class SincronizarViajeCommandHandler : IRequestHandler<SincronizarViajeCommand, ViajeDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public SincronizarViajeCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ViajeDto> Handle(SincronizarViajeCommand request, CancellationToken cancellationToken)
    {
        var viaje = Viaje.Iniciar(
            request.EmpresaId,
            request.CarroId,
            request.SolicitudReservaId,
            request.EmpleadoOperadorId,
            request.FechaInicio,
            request.UbicacionInicioLat,
            request.UbicacionInicioLng,
            request.KilometrajeInicial);

        switch (request.Estado)
        {
            case EstadoViaje.Finalizado:
                if (request.FechaFin is null || request.KilometrajeFinal is null)
                    throw new BadRequestException("Un viaje finalizado debe incluir fecha de fin y kilometraje final.");

                viaje.Finalizar(request.FechaFin.Value, request.UbicacionFinLat, request.UbicacionFinLng, request.KilometrajeFinal.Value);
                break;

            case EstadoViaje.Interrumpido:
                if (request.FechaFin is null)
                    throw new BadRequestException("Un viaje interrumpido debe incluir fecha de fin.");

                viaje.Interrumpir(request.MotivoInterrupcion ?? string.Empty, request.FechaFin.Value,
                    request.UbicacionFinLat, request.UbicacionFinLng, request.KilometrajeFinal);
                break;
        }

        viaje.MarcarSincronizado();

        var carro = await ViajeValidator.ValidarInicioAsync(_unitOfWork, viaje, esSincronizacion: true, cancellationToken);

        if (viaje.KilometrajeFinal is not null)
            carro.AvanzarKilometraje(viaje.KilometrajeFinal.Value);

        await _unitOfWork.Viajes.AddAsync(viaje, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<ViajeDto>(viaje);
    }
}
