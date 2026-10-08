using APIGasoline.Application.Features.RutasProcesadas.Commands;
using APIGasoline.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace APIGasoline.Application.Features.RutasProcesadas.Notifications;

/// <summary>
/// Se publica cuando cambia algo que afecta la ruta de un viaje:
/// se finaliza, se interrumpe o se sincroniza, llegan puntos GPS
/// o se crea, edita o borra una carga de combustible.
/// </summary>
public record RutaViajeModificadaNotification(int ViajeId) : INotification;

/// <summary>
/// Reprocesa la ruta si el viaje ya está cerrado.
/// Un fallo aquí NO deshace la operación original (cerrar el viaje, subir puntos, etc.):
/// ya se guardó antes de publicar. Solo se registra en el log y la ruta se puede recalcular después.
/// </summary>
public class ProcesarRutaAlModificarViajeHandler : INotificationHandler<RutaViajeModificadaNotification>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISender _sender;
    private readonly ILogger<ProcesarRutaAlModificarViajeHandler> _logger;

    public ProcesarRutaAlModificarViajeHandler(
        IUnitOfWork unitOfWork,
        ISender sender,
        ILogger<ProcesarRutaAlModificarViajeHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _sender = sender;
        _logger = logger;
    }

    public async Task Handle(RutaViajeModificadaNotification notification, CancellationToken cancellationToken)
    {
        try
        {
            // Sin el token del cliente: si se desconecta, la operación principal ya se guardó
            // y conviene que el resumen también quede consistente.
            var viaje = await _unitOfWork.Viajes.GetByIdAsync(notification.ViajeId, CancellationToken.None);

            if (viaje is null || viaje.EstaEnCurso)
                return;

            await _sender.Send(new ProcesarRutaViajeCommand(viaje.ViajeId), CancellationToken.None);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "No se pudo procesar la ruta del viaje {ViajeId}. Se puede recalcular con el endpoint de rutas procesadas.",
                notification.ViajeId);
        }
    }
}
