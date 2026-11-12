using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Application.Common.Interfaces;
using APIGasoline.Application.Features.PuntosRuta.Common;
using APIGasoline.Application.Features.Viajes.Common;
using APIGasoline.Application.Features.PuntosRuta.DTOs;
using APIGasoline.Domain.Entities;
using APIGasoline.Domain.Interfaces;
using APIGasoline.Application.Features.CargasCombustible.Notifications;
using APIGasoline.Application.Common.Messaging;
using MediatR;
using APIGasoline.Application.Features.RutasProcesadas.Notifications;

namespace APIGasoline.Application.Features.PuntosRuta.Commands;

/// <summary>
/// Registra varios puntos GPS de un viaje en una sola llamada.
/// - La empresa se toma del viaje (no se confía en la app).
/// - Es idempotente: los puntos con un timestamp ya registrado para el viaje se ignoran,
///   así la app puede reenviar un lote si se cortó la conexión. Si el mismo lote llega dos veces
///   al mismo tiempo, el índice único (viaje, timestamp) frena el segundo y aquí se vuelve a
///   filtrar contra lo ya guardado (F010): responde como reintento, sin duplicar.
/// - Todo el lote se guarda en un solo SaveChanges (si un punto es inválido, no se guarda ninguno).
/// - Los puntos que caen dentro de una pausa del viaje se descartan (la app apaga el GPS en pausa;
///   esto es solo la red de seguridad para que la ruta no cuente el tiempo detenido).
/// </summary>
public sealed record RegistrarLotePuntosRutaCommand(
    Guid ViajeId,
    IReadOnlyList<PuntoRutaNuevoDto> Puntos) : ICommand<LotePuntosRutaResultadoDto>;

public sealed class RegistrarLotePuntosRutaCommandHandler : ICommandHandler<RegistrarLotePuntosRutaCommand, LotePuntosRutaResultadoDto>
{
    public const int MaxPuntosPorLote = 1000;

    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _dateTime;
    private readonly IPublisher _publisher;
    private readonly IUsuarioActual _usuarioActual;

    public RegistrarLotePuntosRutaCommandHandler(IUnitOfWork unitOfWork, IDateTimeProvider dateTime, IPublisher publisher, IUsuarioActual usuarioActual)
    {
        _unitOfWork = unitOfWork;
        _dateTime = dateTime;
        _publisher = publisher;
        _usuarioActual = usuarioActual;
    }

    public async Task<LotePuntosRutaResultadoDto> Handle(RegistrarLotePuntosRutaCommand request, CancellationToken cancellationToken)
    {
        if (request.Puntos.Count == 0)
            throw new BadRequestException("El lote no contiene puntos.");

        if (request.Puntos.Count > MaxPuntosPorLote)
            throw new BadRequestException($"Un lote no puede tener más de {MaxPuntosPorLote} puntos.");

        var viaje = await _unitOfWork.Viajes.GetByIdAsync(request.ViajeId, cancellationToken)
            ?? throw new NotFoundException($"No existe un viaje con el id {request.ViajeId}.");

        // Solo el operador del viaje manda su GPS (o un supervisor con viaje.corregir_cerrado)
        await AccesoViaje.ValidarOperadorAsync(_usuarioActual, viaje, cancellationToken);

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
                punto.PrecisionMetros,
                punto.Altitud,
                punto.Velocidad);

            // Los puntos deben caer dentro del periodo del viaje (con tolerancia)
            if (!VentanaViaje.Contiene(puntoRuta.TimestampCaptura, viaje.FechaInicio, viaje.FechaFin, ahora))
                throw new BadRequestException(
                    $"El punto capturado el {puntoRuta.TimestampCaptura:yyyy-MM-dd HH:mm:ss} está fuera del periodo del viaje.");

            puntos.Add(puntoRuta);
        }

        var pausas = await _unitOfWork.ViajePausas.GetByViajesAsync([viaje.ViajeId], cancellationToken);
        var fueraDePausa = puntos.Where(p => !pausas.Any(pausa => pausa.Contiene(p.TimestampCaptura))).ToList();
        var enPausa = puntos.Count - fueraDePausa.Count;

        var nuevos = await GuardarNuevosAsync(viaje.ViajeId, fueraDePausa, cancellationToken);

        if (nuevos.Count > 0)
        {
            // Puntos que llegan después de cerrar el viaje (por ejemplo, sincronización offline)
            if (!viaje.EstaEnCurso)
                await _publisher.Publish(new RutaViajeModificadaNotification(viaje.ViajeId), cancellationToken);
            // Viaje en curso: con los puntos nuevos se pueden ubicar cargas que llegaron sin GPS
            else
                await _publisher.Publish(new UbicacionCargasPendienteNotification(viaje.ViajeId), cancellationToken);
        }

        return new LotePuntosRutaResultadoDto(
            viaje.ViajeId,
            request.Puntos.Count,
            nuevos.Count,
            request.Puntos.Count - nuevos.Count - enPausa,
            enPausa);
    }

    /// <summary>
    /// Guarda los puntos que todavía no existen. Si otra petición guardó algunos en el mismo instante
    /// (índice único), vuelve a leer y lo intenta una vez más solo con los que siguen faltando.
    /// </summary>
    private async Task<IReadOnlyList<PuntoRuta>> GuardarNuevosAsync(Guid viajeId, IReadOnlyList<PuntoRuta> candidatos, CancellationToken cancellationToken)
    {
        for (var intento = 1; ; intento++)
        {
            var existentes = await _unitOfWork.PuntosRuta.GetTimestampsByViajeAsync(viajeId, cancellationToken);
            var nuevos = DeduplicadorPuntosRuta.FiltrarNuevos(candidatos, existentes);

            if (nuevos.Count == 0)
                return nuevos;

            await _unitOfWork.PuntosRuta.AddRangeAsync(nuevos, cancellationToken);

            if (intento == 2)
            {
                await _unitOfWork.SaveChangesAsync(cancellationToken); // si vuelve a chocar: 409
                return nuevos;
            }

            if (await _unitOfWork.SaveChangesSalvoDuplicadoAsync(cancellationToken))
                return nuevos;
        }
    }
}
