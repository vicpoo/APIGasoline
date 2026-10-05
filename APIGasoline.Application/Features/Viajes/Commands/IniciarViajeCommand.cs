using APIGasoline.Application.Features.Viajes.Common;
using APIGasoline.Application.Features.Viajes.DTOs;
using APIGasoline.Domain.Entities;
using APIGasoline.Domain.Interfaces;
using AutoMapper;
using MediatR;

namespace APIGasoline.Application.Features.Viajes.Commands;

/// <summary>
/// Inicia un viaje en línea (botón "Iniciar viaje" de la app).
/// </summary>
public record IniciarViajeCommand(
    int EmpresaId,
    int CarroId,
    int? SolicitudReservaId,
    int EmpleadoOperadorId,
    DateTime FechaInicio,
    decimal? UbicacionInicioLat,
    decimal? UbicacionInicioLng,
    decimal KilometrajeInicial) : IRequest<ViajeDto>;

public class IniciarViajeCommandHandler : IRequestHandler<IniciarViajeCommand, ViajeDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public IniciarViajeCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ViajeDto> Handle(IniciarViajeCommand request, CancellationToken cancellationToken)
    {
        // El dominio valida los datos
        var viaje = Viaje.Iniciar(
            request.EmpresaId,
            request.CarroId,
            request.SolicitudReservaId,
            request.EmpleadoOperadorId,
            request.FechaInicio,
            request.UbicacionInicioLat,
            request.UbicacionInicioLng,
            request.KilometrajeInicial);

        await ViajeValidator.ValidarInicioAsync(_unitOfWork, viaje, esSincronizacion: false, cancellationToken);

        await _unitOfWork.Viajes.AddAsync(viaje, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<ViajeDto>(viaje);
    }
}
