using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Application.Common.Interfaces;
using APIGasoline.Application.Common.Storage;
using APIGasoline.Application.Features.Carros.DTOs;
using APIGasoline.Domain.Enums;
using APIGasoline.Domain.Interfaces;
using AutoMapper;
using MediatR;

namespace APIGasoline.Application.Features.Carros.Commands;

/// <summary>
/// Actualiza los datos del carro. La foto NO se cambia aquí: usa SubirFotoVehiculoCommand / EliminarFotoVehiculoCommand.
/// </summary>
public record UpdateCarroCommand(
    int CarroId,
    int EmpresaId,
    string Marca,
    string Modelo,
    int? Anio,
    string? Color,
    string Placas,
    decimal? CapacidadTanque,
    decimal KilometrajeActual) : IRequest<CarroDto>;

public class UpdateCarroCommandHandler : IRequestHandler<UpdateCarroCommand, CarroDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IFileStorageService _storage;
    private readonly IDateTimeProvider _dateTime;

    public UpdateCarroCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, IFileStorageService storage, IDateTimeProvider dateTime)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _storage = storage;
        _dateTime = dateTime;
    }

    public async Task<CarroDto> Handle(UpdateCarroCommand request, CancellationToken cancellationToken)
    {
        var carro = await _unitOfWork.Carros.GetByIdAsync(request.CarroId, cancellationToken)
            ?? throw new NotFoundException($"No existe un carro con el id {request.CarroId}.");

        // El dominio valida y normaliza los datos
        carro.CambiarEmpresa(request.EmpresaId);
        carro.CambiarDatos(request.Marca, request.Modelo, request.Anio, request.Color, _dateTime.Now.Year);
        carro.CambiarPlacas(request.Placas);
        carro.CambiarCapacidadTanque(request.CapacidadTanque);
        carro.ActualizarKilometraje(request.KilometrajeActual);
        // El estado NO se cambia aquí: usa /api/cambios-estatus-carro (queda en la bitácora)

        if (!await _unitOfWork.Empresas.ExistsAsync(carro.EmpresaId, cancellationToken))
            throw new NotFoundException($"No existe una empresa con el id {carro.EmpresaId}.");

        if (await _unitOfWork.Carros.PlacasExistsAsync(carro.Placas, carro.CarroId, cancellationToken))
            throw new ConflictException($"Ya existe otro carro con las placas '{carro.Placas}'.");

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await _mapper.Map<CarroDto>(carro).ConUrlsAsync(_storage, cancellationToken);
    }
}
