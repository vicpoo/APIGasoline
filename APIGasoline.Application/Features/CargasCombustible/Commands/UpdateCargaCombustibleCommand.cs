using APIGasoline.Application.Common.Auditoria;
using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Application.Common.Interfaces;
using APIGasoline.Application.Common.Models;
using APIGasoline.Application.Common.Storage;
using APIGasoline.Application.Features.CargasCombustible.Common;
using APIGasoline.Application.Features.CargasCombustible.DTOs;
using APIGasoline.Domain.Enums;
using APIGasoline.Domain.Interfaces;
using AutoMapper;
using MediatR;
using APIGasoline.Application.Features.RutasProcesadas.Notifications;

namespace APIGasoline.Application.Features.CargasCombustible.Commands;

/// <summary>
/// Corrige los datos de una carga. No se puede mover a otro viaje.
/// Si se manda <see cref="NuevaFotoComprobante"/>, reemplaza el comprobante; si es null, se conserva el actual.
/// Es una corrección: el motivo es obligatorio, se aplican las reglas de <see cref="ReglasCorreccion"/>,
/// la carga queda marcada como corregida y el valor anterior se conserva en auditoria_evento.
/// </summary>
public record UpdateCargaCombustibleCommand(
    Guid CargaCombustibleId,
    decimal Litros,
    decimal? Monto,
    ArchivoEntrada? NuevaFotoComprobante,
    DateTime FechaCarga,
    decimal? UbicacionLat,
    decimal? UbicacionLng,
    string? Motivo) : IRequest<CargaCombustibleDto>;

public class UpdateCargaCombustibleCommandHandler : IRequestHandler<UpdateCargaCombustibleCommand, CargaCombustibleDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IDateTimeProvider _dateTime;
    private readonly IFileStorageService _storage;
    private readonly IPublisher _publisher;
    private readonly IUsuarioActual _usuarioActual;
    private readonly IContextoAuditoria _auditoria;

    public UpdateCargaCombustibleCommandHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IDateTimeProvider dateTime,
        IFileStorageService storage,
        IPublisher publisher,
        IUsuarioActual usuarioActual,
        IContextoAuditoria auditoria)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _dateTime = dateTime;
        _storage = storage;
        _publisher = publisher;
        _usuarioActual = usuarioActual;
        _auditoria = auditoria;
    }

    public async Task<CargaCombustibleDto> Handle(UpdateCargaCombustibleCommand request, CancellationToken cancellationToken)
    {
        // Si viene archivo nuevo, se valida antes de tocar la BD
        var extension = request.NuevaFotoComprobante is null
            ? null
            : await ArchivoValidator.ValidarAsync(request.NuevaFotoComprobante, TiposArchivo.Imagenes, "el comprobante", cancellationToken);

        var carga = await _unitOfWork.CargasCombustible.GetByIdAsync(request.CargaCombustibleId, cancellationToken)
            ?? throw new NotFoundException($"No existe una carga de combustible con el id {request.CargaCombustibleId}.");

        var viaje = await _unitOfWork.Viajes.GetByIdAsync(carga.ViajeId, cancellationToken)
            ?? throw new NotFoundException($"No existe un viaje con el id {carga.ViajeId}.");

        // Quién puede corregir y con qué motivo
        var motivo = ReglasCorreccion.Validar(_usuarioActual, viaje, request.Motivo);

        var comprobanteAnterior = carga.FotoComprobante;
        var llave = extension is null
            ? comprobanteAnterior
            : LlavesArchivo.ComprobanteCarga(carga.EmpresaId, carga.ViajeId, carga.CargaCombustibleId, extension, Guid.NewGuid());

        // El dominio valida los datos
        carga.CambiarDatos(
            request.Litros,
            request.Monto,
            llave,
            request.FechaCarga,
            request.UbicacionLat,
            request.UbicacionLng);

        await CargaCombustibleValidator.ValidarAsync(_unitOfWork, _dateTime, viaje, carga, cancellationToken);

        carga.MarcarCorregida();

        if (request.NuevaFotoComprobante is not null)
            carga.MarcarComprobanteSubido(_dateTime.Now);

        // El interceptor guarda el valor anterior y el nuevo con esta acción y motivo
        _auditoria.Registrar(carga.CargaCombustibleId, AccionAuditoria.Corregir, motivo);

        if (request.NuevaFotoComprobante is null)
        {
            // Sin archivo nuevo: solo cambios en BD
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        else
        {
            // Con archivo nuevo: subir + guardar en transacción (con compensación si falla)
            await ArchivoTransaccion.SubirYGuardarAsync(_unitOfWork, _storage, request.NuevaFotoComprobante, llave,
                _ => Task.CompletedTask, // los cambios ya están en la entidad; el Commit hace SaveChanges
                cancellationToken);

            // El comprobante anterior NO se borra: es evidencia y su llave queda en el historial de auditoría
        }

        // Los litros cambian el rendimiento km/L de un viaje ya cerrado
        if (!viaje.EstaEnCurso)
            await _publisher.Publish(new RutaViajeModificadaNotification(viaje.ViajeId), cancellationToken);

        return await _mapper.Map<CargaCombustibleDto>(carga).ConUrlsAsync(_storage, cancellationToken);
    }
}
