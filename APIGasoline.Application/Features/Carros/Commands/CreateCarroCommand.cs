using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Application.Features.Carros.DTOs;
using APIGasoline.Domain.Entities;
using APIGasoline.Domain.Enums;
using APIGasoline.Domain.Interfaces;
using AutoMapper;
using MediatR;

namespace APIGasoline.Application.Features.Carros.Commands;

public record CreateCarroCommand(
    int EmpresaId,
    string Marca,
    string Modelo,
    int? Anio,
    string? Color,
    string Placas,
    string? FotoVehiculo,
    decimal? CapacidadTanque,
    decimal KilometrajeActual,
    EstadoCarro Estado) : IRequest<CarroDto>;

public class CreateCarroCommandHandler : IRequestHandler<CreateCarroCommand, CarroDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateCarroCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
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
            request.FotoVehiculo,
            request.CapacidadTanque,
            request.KilometrajeActual,
            request.Estado);

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
