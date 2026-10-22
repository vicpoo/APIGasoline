using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Application.Common.Interfaces;
using APIGasoline.Application.Common.Models;
using APIGasoline.Application.Common.Storage;
using APIGasoline.Application.Features.CargasCombustible.Common;
using APIGasoline.Application.Features.CargasCombustible.DTOs;
using APIGasoline.Domain.Entities;
using APIGasoline.Domain.Interfaces;
using AutoMapper;
using MediatR;
using APIGasoline.Application.Features.RutasProcesadas.Notifications;

namespace APIGasoline.Application.Features.CargasCombustible.Commands;

/// <summary>
/// Registra una carga dentro de un viaje. La empresa se toma del viaje.
/// El comprobante (foto del ticket) es obligatorio: se sube al almacenamiento y en la BD solo se guarda su llave.
/// </summary>
public record CreateCargaCombustibleCommand(
    Guid ViajeId,
    decimal Litros,
    decimal? Monto,
    ArchivoEntrada FotoComprobante,
    DateTime FechaCarga,
    decimal? UbicacionLat,
    decimal? UbicacionLng,
    Guid? CentroCostoId = null) : IRequest<CargaCombustibleDto>;

public class CreateCargaCombustibleCommandHandler : IRequestHandler<CreateCargaCombustibleCommand, CargaCombustibleDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IDateTimeProvider _dateTime;
    private readonly IFileStorageService _storage;
    private readonly IPublisher _publisher;

    public CreateCargaCombustibleCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, IDateTimeProvider dateTime, IFileStorageService storage, IPublisher publisher)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _dateTime = dateTime;
        _storage = storage;
        _publisher = publisher;
    }

    public async Task<CargaCombustibleDto> Handle(CreateCargaCombustibleCommand request, CancellationToken cancellationToken)
    {
        var extension = await ArchivoValidator.ValidarAsync(request.FotoComprobante, TiposArchivo.Imagenes, "el comprobante", cancellationToken);

        var viaje = await _unitOfWork.Viajes.GetByIdAsync(request.ViajeId, cancellationToken)
            ?? throw new NotFoundException($"No existe un viaje con el id {request.ViajeId}.");

        // El id de la carga se genera antes para guardar el comprobante en su carpeta
        var cargaId = Guid.CreateVersion7();
        var llave = LlavesArchivo.ComprobanteCarga(viaje.EmpresaId, viaje.ViajeId, cargaId, extension, Guid.NewGuid());

        // El dominio valida los datos (incluida la llave del comprobante)
        var carga = CargaCombustible.Crear(
            viaje.EmpresaId,
            viaje.ViajeId,
            request.Litros,
            request.Monto,
            llave,
            request.FechaCarga,
            request.UbicacionLat,
            request.UbicacionLng,
            cargaId,
            request.CentroCostoId);

        // Primero todas las validaciones: así no se sube un archivo que después se rechaza
        await CargaCombustibleValidator.ValidarAsync(_unitOfWork, _dateTime, viaje, carga, cancellationToken);

        // El comprobante se sube junto con los datos; si la subida falla, no se guarda la carga
        carga.MarcarComprobanteSubido(_dateTime.Now);

        // Sube el comprobante y guarda la carga en una transacción (con compensación si falla)
        await ArchivoTransaccion.SubirYGuardarAsync(_unitOfWork, _storage, request.FotoComprobante, llave,
            ct => _unitOfWork.CargasCombustible.AddAsync(carga, ct),
            cancellationToken);

        // Los litros cambian el rendimiento km/L de un viaje ya cerrado
        if (!viaje.EstaEnCurso)
            await _publisher.Publish(new RutaViajeModificadaNotification(viaje.ViajeId), cancellationToken);

        return await _mapper.Map<CargaCombustibleDto>(carga).ConUrlsAsync(_storage, cancellationToken);
    }
}
