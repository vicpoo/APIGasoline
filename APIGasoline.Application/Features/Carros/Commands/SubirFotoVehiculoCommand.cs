using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Application.Common.Interfaces;
using APIGasoline.Application.Common.Models;
using APIGasoline.Application.Common.Storage;
using APIGasoline.Application.Features.Carros.DTOs;
using APIGasoline.Domain.Interfaces;
using AutoMapper;
using MediatR;

namespace APIGasoline.Application.Features.Carros.Commands;

/// <summary>
/// Sube (o reemplaza) la foto del vehículo. En la BD solo se guarda la llave del archivo.
/// </summary>
public record SubirFotoVehiculoCommand(Guid CarroId, ArchivoEntrada Archivo) : IRequest<CarroDto>;

public class SubirFotoVehiculoCommandHandler : IRequestHandler<SubirFotoVehiculoCommand, CarroDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IFileStorageService _storage;

    public SubirFotoVehiculoCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, IFileStorageService storage)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _storage = storage;
    }

    public async Task<CarroDto> Handle(SubirFotoVehiculoCommand request, CancellationToken cancellationToken)
    {
        var extension = await ArchivoValidator.ValidarAsync(request.Archivo, TiposArchivo.Imagenes, "la foto del vehículo", cancellationToken);

        var carro = await _unitOfWork.Carros.GetByIdAsync(request.CarroId, cancellationToken)
            ?? throw new NotFoundException($"No existe un carro con el id {request.CarroId}.");

        var fotoAnterior = carro.FotoVehiculo;
        var llave = LlavesArchivo.FotoVehiculo(carro.EmpresaId, carro.CarroId, extension, Guid.NewGuid());

        // Sube el archivo y guarda la llave en una transacción (con compensación si falla)
        await ArchivoTransaccion.SubirYGuardarAsync(_unitOfWork, _storage, request.Archivo, llave, _ =>
        {
            carro.CambiarFotoVehiculo(llave); // el dominio valida la longitud
            return Task.CompletedTask;
        }, cancellationToken);

        // Ya confirmado en BD: se borra la foto anterior
        await _storage.IntentarEliminarAsync(fotoAnterior);

        return await _mapper.Map<CarroDto>(carro).ConUrlsAsync(_storage, cancellationToken);
    }
}
