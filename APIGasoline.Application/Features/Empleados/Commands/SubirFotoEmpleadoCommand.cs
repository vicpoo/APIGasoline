using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Application.Common.Interfaces;
using APIGasoline.Application.Common.Models;
using APIGasoline.Application.Common.Storage;
using APIGasoline.Application.Features.Empleados.Common;
using APIGasoline.Application.Features.Empleados.DTOs;
using APIGasoline.Domain.Interfaces;
using AutoMapper;
using MediatR;

namespace APIGasoline.Application.Features.Empleados.Commands;

/// <summary>
/// Sube (o reemplaza) la foto de perfil o la licencia del empleado.
/// En la BD solo se guarda la llave del archivo.
/// </summary>
public record SubirFotoEmpleadoCommand(Guid EmpleadoId, TipoFotoEmpleado Tipo, ArchivoEntrada Archivo) : IRequest<EmpleadoDto>;

public class SubirFotoEmpleadoCommandHandler : IRequestHandler<SubirFotoEmpleadoCommand, EmpleadoDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IFileStorageService _storage;
    private readonly IDateTimeProvider _dateTime;

    public SubirFotoEmpleadoCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, IFileStorageService storage, IDateTimeProvider dateTime)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _storage = storage;
        _dateTime = dateTime;
    }

    public async Task<EmpleadoDto> Handle(SubirFotoEmpleadoCommand request, CancellationToken cancellationToken)
    {
        // La licencia puede ser foto o PDF; el perfil solo imagen
        var extension = request.Tipo == TipoFotoEmpleado.Licencia
            ? await ArchivoValidator.ValidarAsync(request.Archivo, TiposArchivo.ImagenesYPdf, "la licencia", cancellationToken)
            : await ArchivoValidator.ValidarAsync(request.Archivo, TiposArchivo.Imagenes, "la foto de perfil", cancellationToken);

        var empleado = await _unitOfWork.Empleados.GetByIdAsync(request.EmpleadoId, cancellationToken)
            ?? throw new NotFoundException($"No existe un empleado con el id {request.EmpleadoId}.");

        string? archivoAnterior;
        string llave;

        if (request.Tipo == TipoFotoEmpleado.Licencia)
        {
            archivoAnterior = empleado.FotoLicencia;
            llave = LlavesArchivo.FotoLicenciaEmpleado(empleado.EmpresaId, empleado.EmpleadoId, extension, Guid.NewGuid());
        }
        else
        {
            archivoAnterior = empleado.FotoPerfil;
            llave = LlavesArchivo.FotoPerfilEmpleado(empleado.EmpresaId, empleado.EmpleadoId, extension, Guid.NewGuid());
        }

        // Sube el archivo y guarda la llave en una transacción (con compensación si falla)
        await ArchivoTransaccion.SubirYGuardarAsync(_unitOfWork, _storage, request.Archivo, llave, _ =>
        {
            if (request.Tipo == TipoFotoEmpleado.Licencia)
                empleado.CambiarFotoLicencia(llave, _dateTime.Now); // HU-LOG-01: fecha de carga
            else
                empleado.CambiarFotoPerfil(llave);

            return Task.CompletedTask;
        }, cancellationToken);

        // Ya confirmado en BD: se borra el archivo anterior
        await _storage.IntentarEliminarAsync(archivoAnterior);

        return await _mapper.Map<EmpleadoDto>(empleado).ConUrlsAsync(_storage, cancellationToken);
    }
}
