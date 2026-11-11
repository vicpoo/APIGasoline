using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Application.Common.Interfaces;
using APIGasoline.Application.Common.Models;
using APIGasoline.Application.Common.Storage;
using APIGasoline.Application.Features.CargasCombustible.Common;
using APIGasoline.Application.Features.CargasCombustible.DTOs;
using APIGasoline.Application.Features.CargasCombustible.Notifications;
using APIGasoline.Application.Features.Viajes.Common;
using APIGasoline.Domain.Entities;
using APIGasoline.Domain.Interfaces;
using AutoMapper;
using APIGasoline.Application.Common.Messaging;
using MediatR;
using APIGasoline.Application.Features.RutasProcesadas.Notifications;

namespace APIGasoline.Application.Features.CargasCombustible.Commands;

/// <summary>
/// Registra una carga dentro de un viaje. La empresa se toma del viaje.
/// El comprobante (foto del ticket) es obligatorio: se sube al almacenamiento y en la BD solo se guarda su llave.
/// Solo el operador del viaje (o quien tenga viaje.corregir_cerrado) puede registrar cargas en él.
/// Idempotente: si llega <see cref="CargaCombustibleId"/> (UUID generado en el teléfono) y esa carga ya
/// existe, se regresa la existente sin duplicarla ni volver a subir el ticket.
/// </summary>
public sealed record CreateCargaCombustibleCommand(
    Guid ViajeId,
    decimal Litros,
    decimal? Monto,
    ArchivoEntrada FotoComprobante,
    DateTime FechaCarga,
    decimal? UbicacionLat,
    decimal? UbicacionLng,
    Guid? CentroCostoId = null,
    Guid? CargaCombustibleId = null) : ICommand<CargaCombustibleDto>;

public sealed class CreateCargaCombustibleCommandHandler : ICommandHandler<CreateCargaCombustibleCommand, CargaCombustibleDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IDateTimeProvider _dateTime;
    private readonly IFileStorageService _storage;
    private readonly IPublisher _publisher;
    private readonly IUsuarioActual _usuarioActual;

    public CreateCargaCombustibleCommandHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IDateTimeProvider dateTime,
        IFileStorageService storage,
        IPublisher publisher,
        IUsuarioActual usuarioActual)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _dateTime = dateTime;
        _storage = storage;
        _publisher = publisher;
        _usuarioActual = usuarioActual;
    }

    public async Task<CargaCombustibleDto> Handle(CreateCargaCombustibleCommand request, CancellationToken cancellationToken)
    {
        var extension = await ArchivoValidator.ValidarAsync(request.FotoComprobante, TiposArchivo.Imagenes, "el comprobante", cancellationToken);

        var viaje = await _unitOfWork.Viajes.GetByIdAsync(request.ViajeId, cancellationToken)
            ?? throw new NotFoundException($"No existe un viaje con el id {request.ViajeId}.");

        // Misma empresa y, para el viaje de otro, viaje.corregir_cerrado (igual que pausar o finalizar)
        await AccesoViaje.ValidarOperadorAsync(_usuarioActual, viaje, cancellationToken);

        // Reintento: la carga ya se guardó pero la respuesta no llegó al teléfono
        if (request.CargaCombustibleId is { } idCliente
            && await _unitOfWork.CargasCombustible.GetByIdAsync(idCliente, cancellationToken) is { } existente)
        {
            ReintentoRegistro.ValidarCarga(existente, viaje.ViajeId);
            return await _mapper.Map<CargaCombustibleDto>(existente).ConUrlsAsync(_storage, cancellationToken);
        }

        // El id de la carga se genera antes para guardar el comprobante en su carpeta
        // (o se usa el que mandó la app)
        var cargaId = request.CargaCombustibleId ?? Guid.CreateVersion7();
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
        // (con el viaje cerrado, esta notificación también completa la ubicación si faltó)
        if (!viaje.EstaEnCurso)
            await _publisher.Publish(new RutaViajeModificadaNotification(viaje.ViajeId), cancellationToken);
        // Sin GPS al registrarla: se ubica con el punto de la ruta más cercano a su hora
        else if (carga.UbicacionLat is null)
            await _publisher.Publish(new UbicacionCargasPendienteNotification(viaje.ViajeId), cancellationToken);

        return await _mapper.Map<CargaCombustibleDto>(carga).ConUrlsAsync(_storage, cancellationToken);
    }
}
