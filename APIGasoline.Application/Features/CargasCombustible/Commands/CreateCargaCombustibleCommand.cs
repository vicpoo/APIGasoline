using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Application.Common.Interfaces;
using APIGasoline.Application.Features.CargasCombustible.Common;
using APIGasoline.Application.Features.CargasCombustible.DTOs;
using APIGasoline.Domain.Entities;
using APIGasoline.Domain.Interfaces;
using AutoMapper;
using MediatR;

namespace APIGasoline.Application.Features.CargasCombustible.Commands;

/// <summary>
/// Registra una carga dentro de un viaje. La empresa se toma del viaje.
/// </summary>
public record CreateCargaCombustibleCommand(
    int ViajeId,
    decimal Litros,
    decimal? Monto,
    string FotoComprobante,
    DateTime FechaCarga,
    decimal? UbicacionLat,
    decimal? UbicacionLng,
    bool Sincronizado) : IRequest<CargaCombustibleDto>;

public class CreateCargaCombustibleCommandHandler : IRequestHandler<CreateCargaCombustibleCommand, CargaCombustibleDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IDateTimeProvider _dateTime;

    public CreateCargaCombustibleCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, IDateTimeProvider dateTime)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _dateTime = dateTime;
    }

    public async Task<CargaCombustibleDto> Handle(CreateCargaCombustibleCommand request, CancellationToken cancellationToken)
    {
        var viaje = await _unitOfWork.Viajes.GetByIdAsync(request.ViajeId, cancellationToken)
            ?? throw new NotFoundException($"No existe un viaje con el id {request.ViajeId}.");

        // El dominio valida los datos
        var carga = CargaCombustible.Crear(
            viaje.EmpresaId,
            viaje.ViajeId,
            request.Litros,
            request.Monto,
            request.FotoComprobante,
            request.FechaCarga,
            request.UbicacionLat,
            request.UbicacionLng,
            request.Sincronizado);

        await CargaCombustibleValidator.ValidarAsync(_unitOfWork, _dateTime, viaje, carga, cancellationToken);

        await _unitOfWork.CargasCombustible.AddAsync(carga, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<CargaCombustibleDto>(carga);
    }
}
