using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Application.Common.Interfaces;
using APIGasoline.Application.Features.Carros.DTOs;
using APIGasoline.Domain.Entities;
using APIGasoline.Domain.Enums;
using APIGasoline.Domain.Interfaces;
using AutoMapper;
using MediatR;

namespace APIGasoline.Application.Features.Carros.Commands;

/// <summary>
/// Crea el carro sin foto. La foto se sube después con <see cref="SubirFotoVehiculoCommand"/>
/// (PUT /api/carros/{id}/foto), porque la llave del archivo usa el id del carro.
/// </summary>
public record CreateCarroCommand(
    Guid EmpresaId,
    string Marca,
    string Modelo,
    int? Anio,
    string? Color,
    string Placas,
    decimal? CapacidadTanque,
    decimal KilometrajeActual,
    EstadoCarro Estado,
    TipoVehiculo TipoVehiculo = TipoVehiculo.Carro) : IRequest<CarroDto>;

public class CreateCarroCommandHandler : IRequestHandler<CreateCarroCommand, CarroDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IDateTimeProvider _dateTime;

    public CreateCarroCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, IDateTimeProvider dateTime)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _dateTime = dateTime;
    }

    public async Task<CarroDto> Handle(CreateCarroCommand request, CancellationToken cancellationToken)
    {
        // El dominio valida y normaliza los datos
        var carro = Carro.Crear(
            request.EmpresaId,
            request.Marca,
            request.Modelo,
            request.Anio,
            request.Color,
            request.Placas,
            null, // foto: se sube con SubirFotoVehiculoCommand
            request.CapacidadTanque,
            request.KilometrajeActual,
            _dateTime.Now.Year,
            request.Estado,
            request.TipoVehiculo);

        // Las empresas eliminadas (soft delete) no cuentan como existentes
        if (!await _unitOfWork.Empresas.ExistsAsync(carro.EmpresaId, cancellationToken))
            throw new NotFoundException($"No existe una empresa con el id {carro.EmpresaId}.");

        if (await _unitOfWork.Carros.PlacasExistsAsync(carro.Placas, null, cancellationToken))
            throw new ConflictException($"Ya existe un carro con las placas '{carro.Placas}'.");

        await _unitOfWork.Carros.AddAsync(carro, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // CarroId y FechaAlta ya vienen asignados por la base de datos
        return _mapper.Map<CarroDto>(carro);
    }
}
