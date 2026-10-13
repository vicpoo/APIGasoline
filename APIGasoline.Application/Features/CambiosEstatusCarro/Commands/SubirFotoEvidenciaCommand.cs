using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Application.Common.Interfaces;
using APIGasoline.Application.Common.Models;
using APIGasoline.Application.Common.Storage;
using APIGasoline.Application.Features.CambiosEstatusCarro.DTOs;
using APIGasoline.Domain.Interfaces;
using AutoMapper;
using MediatR;

namespace APIGasoline.Application.Features.CambiosEstatusCarro.Commands;

/// <summary>
/// HU-EST-02: agrega la foto opcional de la falla o del daño a un cambio de estatus ya registrado.
/// Se sube aparte porque desde el celular puede llegar después que los datos (offline).
/// La foto anterior, si había, no se borra (es evidencia).
/// </summary>
public record SubirFotoEvidenciaCommand(Guid CambioEstatusCarroId, ArchivoEntrada Archivo) : IRequest<CambioEstatusCarroDto>;

public class SubirFotoEvidenciaCommandHandler : IRequestHandler<SubirFotoEvidenciaCommand, CambioEstatusCarroDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IFileStorageService _storage;

    public SubirFotoEvidenciaCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, IFileStorageService storage)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _storage = storage;
    }

    public async Task<CambioEstatusCarroDto> Handle(SubirFotoEvidenciaCommand request, CancellationToken cancellationToken)
    {
        var extension = await ArchivoValidator.ValidarAsync(request.Archivo, TiposArchivo.Imagenes, "la foto de evidencia", cancellationToken);

        var cambio = await _unitOfWork.CambiosEstatusCarro.GetByIdParaActualizarAsync(request.CambioEstatusCarroId, cancellationToken)
            ?? throw new NotFoundException($"No existe un cambio de estatus con el id {request.CambioEstatusCarroId}.");

        var llave = LlavesArchivo.EvidenciaCambioEstatus(cambio.EmpresaId, cambio.CarroId, cambio.CambioEstatusCarroId, extension, Guid.NewGuid());

        // Sube el archivo y guarda la llave en una transacción (con compensación si falla)
        await ArchivoTransaccion.SubirYGuardarAsync(_unitOfWork, _storage, request.Archivo, llave, _ =>
        {
            cambio.CambiarFotoEvidencia(llave);
            return Task.CompletedTask;
        }, cancellationToken);

        return await _mapper.Map<CambioEstatusCarroDto>(cambio).ConUrlsAsync(_storage, cancellationToken);
    }
}
