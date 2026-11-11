using APIGasoline.Application.Common.Security;
using APIGasoline.Application.Common.Interfaces;
using APIGasoline.Application.Common.Storage;
using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Application.Features.Viajes.Common;
using APIGasoline.Application.Features.Viajes.DTOs;
using APIGasoline.Domain.Entities;
using APIGasoline.Domain.Enums;
using APIGasoline.Domain.Interfaces;
using AutoMapper;
using APIGasoline.Application.Common.Messaging;
using MediatR;
using APIGasoline.Application.Features.RutasProcesadas.Notifications;

namespace APIGasoline.Application.Features.Viajes.Commands;

/// <summary>
/// Sube un viaje que la app registró sin conexión (offline-first).
/// Puede venir en curso, finalizado o interrumpido. La BD guarda la hora de llegada en fecha_recepcion.
/// Idempotente: si llega <see cref="ViajeId"/> y ese viaje ya existe, se regresa TAL CUAL está en el servidor
/// (no se vuelve a crear ni se cierra). La app compara el estado devuelto: si sigue en curso, manda
/// después sus pausas, cargas y el finalizar como en un viaje en línea.
/// </summary>
public sealed record SincronizarViajeCommand(
    Guid EmpresaId,
    Guid CarroId,
    Guid? SolicitudReservaId,
    Guid EmpleadoOperadorId,
    DateTime FechaInicio,
    decimal? UbicacionInicioLat,
    decimal? UbicacionInicioLng,
    decimal KilometrajeInicial,
    EstadoViaje Estado,
    DateTime? FechaFin,
    decimal? UbicacionFinLat,
    decimal? UbicacionFinLng,
    decimal? KilometrajeFinal,
    string? MotivoInterrupcion,
    Guid? CentroCostoId = null,
    /// <summary>Pausas que se hicieron sin conexión (opcional).</summary>
    IReadOnlyList<DatosPausa>? Pausas = null,
    /// <summary>UUID generado en el teléfono (opcional).</summary>
    Guid? ViajeId = null) : ICommand<ViajeDto>, IRecursoPropio
{
    /// <summary>El operador sube SUS viajes; a nombre de otro, solo con viaje.corregir_cerrado.</summary>
    public bool EsPropio(IUsuarioActual usuario)
        => EmpleadoOperadorId == usuario.EmpleadoId;

    public string PrivilegioParaOtros => Permisos.ViajeCorregirCerrado;
}

public sealed class SincronizarViajeCommandHandler : ICommandHandler<SincronizarViajeCommand, ViajeDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IFileStorageService _storage;
    private readonly IPublisher _publisher;
    private readonly IDateTimeProvider _dateTime;
    private readonly IUsuarioActual _usuarioActual;

    public SincronizarViajeCommandHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IFileStorageService storage,
        IPublisher publisher,
        IDateTimeProvider dateTime,
        IUsuarioActual usuarioActual)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _storage = storage;
        _publisher = publisher;
        _dateTime = dateTime;
        _usuarioActual = usuarioActual;
    }

    public async Task<ViajeDto> Handle(SincronizarViajeCommand request, CancellationToken cancellationToken)
    {
        // Reintento (o el viaje ya se había creado en línea): se regresa como está en el servidor
        if (request.ViajeId is { } viajeId
            && await _unitOfWork.Viajes.GetByIdAsync(viajeId, cancellationToken) is { } existente)
        {
            ReintentoRegistro.ValidarViaje(existente, request.EmpresaId, request.CarroId, request.EmpleadoOperadorId);
            await AccesoViaje.ValidarOperadorAsync(_usuarioActual, existente, cancellationToken);

            var dtoExistente = await _mapper.Map<ViajeDto>(existente).ConUrlsAsync(_storage, cancellationToken);
            return await dtoExistente.ConPausasAsync(_unitOfWork, _mapper, _dateTime.Now, cancellationToken);
        }

        var viaje = Viaje.Iniciar(
            request.EmpresaId,
            request.CarroId,
            request.SolicitudReservaId,
            request.EmpleadoOperadorId,
            request.FechaInicio,
            request.UbicacionInicioLat,
            request.UbicacionInicioLng,
            request.KilometrajeInicial,
            request.CentroCostoId,
            request.ViajeId);

        switch (request.Estado)
        {
            case EstadoViaje.Finalizado:
                if (request.FechaFin is null || request.KilometrajeFinal is null)
                    throw new BadRequestException("Un viaje finalizado debe incluir fecha de fin y kilometraje final.");

                viaje.Finalizar(request.FechaFin.Value, request.UbicacionFinLat, request.UbicacionFinLng, request.KilometrajeFinal.Value);
                break;

            case EstadoViaje.Interrumpido:
                if (request.FechaFin is null)
                    throw new BadRequestException("Un viaje interrumpido debe incluir fecha de fin.");

                viaje.Interrumpir(request.MotivoInterrupcion ?? string.Empty, request.FechaFin.Value,
                    request.UbicacionFinLat, request.UbicacionFinLng, request.KilometrajeFinal);
                break;
        }

        // Pausas hechas sin conexión: el dominio valida que caigan dentro del viaje y no se encimen
        var pausas = viaje.RegistrarPausasSincronizadas(request.Pausas ?? []);

        var carro = await ViajeValidator.ValidarInicioAsync(_unitOfWork, viaje, esSincronizacion: true, cancellationToken);

        if (viaje.KilometrajeFinal is not null)
            carro.AvanzarKilometraje(viaje.KilometrajeFinal.Value);

        await _unitOfWork.Viajes.AddAsync(viaje, cancellationToken);
        await _unitOfWork.ViajePausas.AddRangeAsync(pausas, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Si llegó ya cerrado se procesa su ruta (y se vuelve a procesar cuando suban sus puntos)
        if (!viaje.EstaEnCurso)
            await _publisher.Publish(new RutaViajeModificadaNotification(viaje.ViajeId), cancellationToken);

        var dto = await _mapper.Map<ViajeDto>(viaje).ConUrlsAsync(_storage, cancellationToken);
        return await dto.ConPausasAsync(_unitOfWork, _mapper, _dateTime.Now, cancellationToken);
    }
}
