using APIGasoline.Application.Features.CargasCombustible.Commands;
using APIGasoline.Application.Features.RutasProcesadas.Notifications;
using MediatR;
using Microsoft.Extensions.Logging;

namespace APIGasoline.Application.Features.CargasCombustible.Notifications;

/// <summary>
/// Se publica cuando puede haber cargas del viaje sin ubicación que ya se pueden ubicar:
/// llegó una carga sin ubicación o llegaron puntos GPS de un viaje en curso.
/// (Para viajes cerrados basta con <see cref="RutaViajeModificadaNotification"/>.)
/// </summary>
public sealed record UbicacionCargasPendienteNotification(Guid ViajeId) : INotification;

/// <summary>
/// Cuando cambia la ruta de un viaje cerrado (se finaliza, se interrumpe, se sincroniza o llegan
/// puntos tarde por estar sin internet) se intenta ubicar las cargas que llegaron sin ubicación.
/// Un fallo aquí no deshace la operación original: solo se registra en el log.
/// </summary>
public sealed class CompletarUbicacionCargasHandler :
    INotificationHandler<RutaViajeModificadaNotification>,
    INotificationHandler<UbicacionCargasPendienteNotification>
{
    private readonly ISender _sender;
    private readonly ILogger<CompletarUbicacionCargasHandler> _logger;

    public CompletarUbicacionCargasHandler(ISender sender, ILogger<CompletarUbicacionCargasHandler> logger)
    {
        _sender = sender;
        _logger = logger;
    }

    public Task Handle(RutaViajeModificadaNotification notification, CancellationToken cancellationToken)
        => CompletarUbicacionCargas.IntentarAsync(_sender, _logger, notification.ViajeId);

    public Task Handle(UbicacionCargasPendienteNotification notification, CancellationToken cancellationToken)
        => CompletarUbicacionCargas.IntentarAsync(_sender, _logger, notification.ViajeId);
}

/// <summary>Ejecuta <see cref="CompletarUbicacionCargasCommand"/> sin dejar que un error rompa la operación que lo llamó.</summary>
public static class CompletarUbicacionCargas
{
    public static async Task IntentarAsync(ISender sender, ILogger logger, Guid viajeId)
    {
        try
        {
            // Sin el token del cliente: la operación principal ya se guardó
            await sender.Send(new CompletarUbicacionCargasCommand(viajeId), CancellationToken.None);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "No se pudo completar la ubicación de las cargas del viaje {ViajeId}.", viajeId);
        }
    }
}
