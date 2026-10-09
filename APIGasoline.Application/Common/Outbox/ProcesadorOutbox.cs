using APIGasoline.Application.Common.Interfaces;
using APIGasoline.Domain.Entities;
using APIGasoline.Domain.Interfaces;
using APIGasoline.Domain.Outbox;
using Microsoft.Extensions.Logging;

namespace APIGasoline.Application.Common.Outbox;

/// <summary>
/// Una vuelta del worker: aparta un lote de mensajes pendientes, los envía y guarda el resultado de cada uno.
/// </summary>
public class ProcesadorOutbox
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPushNotificationService _push;
    private readonly IDateTimeProvider _dateTime;
    private readonly OutboxSettings _settings;
    private readonly ILogger<ProcesadorOutbox> _logger;

    public ProcesadorOutbox(
        IUnitOfWork unitOfWork,
        IPushNotificationService push,
        IDateTimeProvider dateTime,
        OutboxSettings settings,
        ILogger<ProcesadorOutbox> logger)
    {
        _unitOfWork = unitOfWork;
        _push = push;
        _dateTime = dateTime;
        _settings = settings;
        _logger = logger;
    }

    /// <returns>Cuántos mensajes se procesaron.</returns>
    public async Task<int> ProcesarLoteAsync(CancellationToken cancellationToken = default)
    {
        var mensajes = await TomarAsync(cancellationToken);

        foreach (var mensaje in mensajes)
        {
            await ProcesarAsync(mensaje, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return mensajes.Count;
    }

    // Transacción corta: solo para apartar los mensajes. El envío ocurre fuera de ella.
    private async Task<IReadOnlyList<OutboxMensaje>> TomarAsync(CancellationToken cancellationToken)
    {
        var ahora = _dateTime.Now;
        var bloqueo = TimeSpan.FromSeconds(_settings.BloqueoSegundos);

        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            var mensajes = await _unitOfWork.OutboxMensajes.TomarPendientesAsync(_settings.TamanoLote, ahora, cancellationToken);

            foreach (var mensaje in mensajes)
                mensaje.MarcarProcesando(ahora, bloqueo);

            await _unitOfWork.CommitTransactionAsync(cancellationToken);
            return mensajes;
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    private async Task ProcesarAsync(OutboxMensaje mensaje, CancellationToken cancellationToken)
    {
        try
        {
            if (mensaje.Tipo != TiposOutbox.NotificacionPush)
            {
                mensaje.RegistrarFallo($"Tipo de mensaje desconocido: {mensaje.Tipo}.", _dateTime.Now);
                return;
            }

            var payload = MensajesPush.Leer(mensaje.Payload);
            var dispositivos = await _unitOfWork.Dispositivos.GetActivosByEmpleadoAsync(payload.EmpleadoDestinatarioId, cancellationToken);

            if (dispositivos.Count == 0)
            {
                // La notificación sigue en la bandeja de la app; solo no hay a qué celular mandarla
                mensaje.MarcarEnviado(_dateTime.Now, "Sin dispositivos activos.");
                return;
            }

            var resultado = await _push.EnviarAsync(
                new MensajePush(
                    dispositivos.Select(d => d.TokenFcm).ToList(),
                    payload.Titulo,
                    payload.Cuerpo,
                    MensajesPush.Datos(payload)),
                cancellationToken);

            var ahora = _dateTime.Now;
            var invalidos = resultado.TokensInvalidos.ToHashSet();

            foreach (var dispositivo in dispositivos)
            {
                if (invalidos.Contains(dispositivo.TokenFcm))
                    dispositivo.Desactivar();
                else if (resultado.Error is null)
                    dispositivo.RegistrarUso(ahora);
            }

            if (resultado.Error is not null)
                mensaje.RegistrarFallo(resultado.Error, ahora);
            else
                mensaje.MarcarEnviado(ahora, resultado.Enviados == 0 ? "Ningún token válido." : null);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // Queda "procesando"; al vencer el bloqueo se vuelve a tomar
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "No se pudo enviar el mensaje del outbox {OutboxMensajeId}.", mensaje.OutboxMensajeId);
            mensaje.RegistrarFallo(ex.Message, _dateTime.Now);
        }
    }
}
