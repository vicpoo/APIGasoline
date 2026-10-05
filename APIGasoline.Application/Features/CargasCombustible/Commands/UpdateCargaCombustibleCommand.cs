using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Application.Common.Interfaces;
using APIGasoline.Application.Features.CargasCombustible.Common;
using APIGasoline.Application.Features.CargasCombustible.DTOs;
using APIGasoline.Domain.Interfaces;
using AutoMapper;
using MediatR;

namespace APIGasoline.Application.Features.CargasCombustible.Commands;

/// <summary>
/// Corrige los datos de una carga. No se puede mover a otro viaje.
/// </summary>
public record UpdateCargaCombustibleCommand(
    int CargaCombustibleId,
    decimal Litros,
    decimal? Monto,
    string FotoComprobante,
    DateTime FechaCarga,
    decimal? UbicacionLat,
    decimal? UbicacionLng) : IRequest<CargaCombustibleDto>;

public class UpdateCargaCombustibleCommandHandler : IRequestHandler<UpdateCargaCombustibleCommand, CargaCombustibleDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IDateTimeProvider _dateTime;

    public UpdateCargaCombustibleCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, IDateTimeProvider dateTime)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _dateTime = dateTime;
    }

    public async Task<CargaCombustibleDto> Handle(UpdateCargaCombustibleCommand request, CancellationToken cancellationToken)
    {
        var carga = await _unitOfWork.CargasCombustible.GetByIdAsync(request.CargaCombustibleId, cancellationToken)
            ?? throw new NotFoundException($"No existe una carga de combustible con el id {request.CargaCombustibleId}.");

        var viaje = await _unitOfWork.Viajes.GetByIdAsync(carga.ViajeId, cancellationToken)
            ?? throw new NotFoundException($"No existe un viaje con el id {carga.ViajeId}.");

        // El dominio valida los datos
        carga.CambiarDatos(
            request.Litros,
            request.Monto,
            request.FotoComprobante,
            request.FechaCarga,
            request.UbicacionLat,
            request.UbicacionLng);

        await CargaCombustibleValidator.ValidarAsync(_unitOfWork, _dateTime, viaje, carga, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<CargaCombustibleDto>(carga);
    }
}
