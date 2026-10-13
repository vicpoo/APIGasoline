using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Application.Common.Interfaces;
using APIGasoline.Application.Common.Models;
using APIGasoline.Application.Common.Storage;
using APIGasoline.Application.Features.Viajes.Common;
using APIGasoline.Application.Features.Viajes.DTOs;
using APIGasoline.Domain.Interfaces;
using AutoMapper;
using MediatR;

namespace APIGasoline.Application.Features.Viajes.Commands;

/// <summary>
/// Sube (o reemplaza) la foto del tablero al iniciar o al terminar el viaje. Respalda el kilometraje capturado.
/// Se sube aparte de los datos del viaje porque desde el celular puede llegar después (offline).
/// En la BD solo se guarda la llave del archivo.
/// </summary>
public record SubirFotoTableroCommand(Guid ViajeId, MomentoTablero Momento, ArchivoEntrada Archivo) : IRequest<ViajeDto>;

public class SubirFotoTableroCommandHandler : IRequestHandler<SubirFotoTableroCommand, ViajeDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IFileStorageService _storage;

    public SubirFotoTableroCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, IFileStorageService storage)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _storage = storage;
    }

    public async Task<ViajeDto> Handle(SubirFotoTableroCommand request, CancellationToken cancellationToken)
    {
        if (!Enum.IsDefined(request.Momento))
            throw new BadRequestException("El momento de la foto debe ser 'inicial' o 'final'.");

        var extension = await ArchivoValidator.ValidarAsync(request.Archivo, TiposArchivo.Imagenes, "la foto del tablero", cancellationToken);

        var viaje = await _unitOfWork.Viajes.GetByIdAsync(request.ViajeId, cancellationToken)
            ?? throw new NotFoundException($"No existe un viaje con el id {request.ViajeId}.");

        var esInicial = request.Momento == MomentoTablero.Inicial;
        var llave = LlavesArchivo.FotoTablero(viaje.EmpresaId, viaje.ViajeId, esInicial ? "inicial" : "final", extension, Guid.NewGuid());

        // Sube el archivo y guarda la llave en una transacción (con compensación si falla)
        await ArchivoTransaccion.SubirYGuardarAsync(_unitOfWork, _storage, request.Archivo, llave, _ =>
        {
            if (esInicial)
                viaje.CambiarFotoTableroInicial(llave);
            else
                viaje.CambiarFotoTableroFinal(llave);

            return Task.CompletedTask;
        }, cancellationToken);

        // Si había una foto anterior NO se borra: es evidencia y su llave queda en el historial de auditoría

        return await _mapper.Map<ViajeDto>(viaje).ConUrlsAsync(_storage, cancellationToken);
    }
}
