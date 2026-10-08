using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Application.Common.Interfaces;
using APIGasoline.Application.Features.PuntosRuta.Common;
using APIGasoline.Application.Features.Viajes.Common;
using APIGasoline.Application.Features.PuntosRuta.DTOs;
using APIGasoline.Domain.Entities;
using APIGasoline.Domain.Interfaces;
using MediatR;
using APIGasoline.Application.Features.RutasProcesadas.Notifications;

namespace APIGasoline.Application.Features.PuntosRuta.Commands;

/// <summary>
/// Registra varios puntos GPS de un viaje en una sola llamada.
/// - La empresa se toma del viaje (no se confía en la app).
/// - Es idempotente: los puntos con un timestamp ya registrado para el viaje se ignoran,
///   así la app puede reenviar un lote si se cortó la conexión.
/// - Todo el lote se guarda en un solo SaveChanges (si un punto es inválido, no se guarda ninguno).
/// </summary>
public record RegistrarLotePuntosRutaCommand(
    int ViajeId,
    bool Sincronizado,
    IReadOnlyList<PuntoRutaNuevoDto> Puntos) : IRequest<LotePuntosRutaResultadoDto>;

public class RegistrarLotePuntosRutaCommandHandler : IRequestHandler<RegistrarLotePuntosRutaCommand, LotePuntosRutaResultadoDto>
{
    public const int MaxPuntosPorLote = 1000;

    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _dateTime;
    private readonly IPublisher _publisher;

    public RegistrarLotePuntosRutaCommandHandler(IUnitOfWork unitOfWork, IDateTimeProvider dateTime, IPublisher publisher)
    {
        _unitOfWork = unitOfWork;
        _dateTime = dateTime;
        _publisher = publisher;
    }

    public async Task<LotePuntosRutaResultadoDto> Handle(RegistrarLotePuntosRutaCommand request, CancellationToken cancellationToken)
    {
        if (request.Puntos.Count == 0)
            throw new BadRequestException("El lote no contiene puntos.");

        if (request.Puntos.Count > MaxPuntosPorLote)
            throw new BadRequestException($"Un lote no puede tener más de {MaxPuntosPorLote} puntos.");

        var viaje = await _unitOfWork.Viajes.GetByIdAsync(request.ViajeId, cancellationToken)
            ?? throw new NotFoundException($"No existe un viaje con el id {request.ViajeId}.");

        var ahora = _dateTime.Now;
        var puntos = new List<PuntoRuta>();

        foreach (var punto in request.Puntos)
        {
            // El dominio valida coordenadas y normaliza el timestamp
            var puntoRuta = PuntoRuta.Crear(
                viaje.EmpresaId,
                viaje.ViajeId,
                punto.Latitud,
                punto.Longitud,
                punto.TimestampCaptura,
                request.Sincronizado,
                punto.PrecisionMetros,
                punto.Altitud,
                punto.Velocidad);

            // Los puntos deben caer dentro del periodo del viaje (con tolerancia)
            if (!VentanaViaje.Contiene(puntoRuta.TimestampCaptura, viaje.FechaInicio, viaje.FechaFin, ahora))
                throw new BadRequestException(
                    $"El punto capturado el {puntoRuta.TimestampCaptura:yyyy-MM-dd HH:mm:ss} está fuera del periodo del viaje.");

            puntos.Add(puntoRuta);
        }

        var existentes = await _unitOfWork.PuntosRuta.GetTimestampsByViajeAsync(viaje.ViajeId, cancellationToken);
        var nuevos = DeduplicadorPuntosRuta.FiltrarNuevos(puntos, existentes);

        if (nuevos.Count > 0)
        {
            await _unitOfWork.PuntosRuta.AddRangeAsync(nuevos, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Puntos que llegan después de cerrar el viaje (por ejemplo, sincronización offline)
            if (!viaje.EstaEnCurso)
                await _publisher.Publish(new RutaViajeModificadaNotification(viaje.ViajeId), cancellationToken);
        }

        return new LotePuntosRutaResultadoDto(
            viaje.ViajeId,
            request.Puntos.Count,
            nuevos.Count,
            request.Puntos.Count - nuevos.Count);
    }
}
