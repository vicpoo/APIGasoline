using APIGasoline.Application.Common.Interfaces;
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Logging;

namespace APIGasoline.Infrastructure.Push;

/// <summary>
/// Envío real de notificaciones push con Firebase Cloud Messaging (FCM).
/// Clasifica la respuesta de cada token para que el outbox sepa qué hacer:
/// - token que ya no existe o no es válido → se reporta para desactivar el dispositivo;
/// - error temporal (Firebase caído, cuota) y no llegó a nadie → el mensaje se reintenta.
/// </summary>
public sealed class FirebasePushService : IPushNotificationService, IDisposable
{
    // Límite de FCM por envío multicast
    private const int MaxTokensPorEnvio = 500;
    private const string NombreApp = "apigasoline";

    private static readonly HashSet<MessagingErrorCode> ErroresDeToken =
    [
        MessagingErrorCode.Unregistered,      // la app se desinstaló o el token expiró
        MessagingErrorCode.InvalidArgument,   // el token no tiene formato válido
        MessagingErrorCode.SenderIdMismatch   // el token es de otro proyecto de Firebase
    ];

    private readonly FirebaseApp _app;
    private readonly FirebaseMessaging _messaging;
    private readonly ILogger<FirebasePushService> _logger;

    public FirebasePushService(PushSettings settings, ILogger<FirebasePushService> logger)
    {
        _logger = logger;

        var opciones = new AppOptions
        {
            Credential = GoogleCredential.FromFile(settings.Firebase.CredencialesPath)
        };

        if (!string.IsNullOrWhiteSpace(settings.Firebase.ProjectId))
            opciones.ProjectId = settings.Firebase.ProjectId;

        _app = FirebaseApp.Create(opciones, NombreApp);
        _messaging = FirebaseMessaging.GetMessaging(_app);
    }

    public async Task<ResultadoPush> EnviarAsync(MensajePush mensaje, CancellationToken cancellationToken = default)
    {
        if (mensaje.Tokens.Count == 0)
            return new ResultadoPush(0, [], null);

        var enviados = 0;
        var invalidos = new List<string>();
        string? errorTemporal = null;

        foreach (var lote in mensaje.Tokens.Chunk(MaxTokensPorEnvio))
        {
            BatchResponse respuesta;

            try
            {
                respuesta = await _messaging.SendEachForMulticastAsync(Construir(mensaje, lote), cancellationToken);
            }
            catch (FirebaseException ex)
            {
                // Falla de todo el envío (credenciales, red, Firebase caído): se reintenta más tarde
                _logger.LogWarning(ex, "Firebase rechazó el envío: {Codigo}.", ex.ErrorCode);
                return ResultadoPush.Fallo($"Firebase ({ex.ErrorCode}): {ex.Message}");
            }

            enviados += respuesta.SuccessCount;

            for (var i = 0; i < respuesta.Responses.Count; i++)
            {
                var resultado = respuesta.Responses[i];

                if (resultado.IsSuccess)
                    continue;

                if (resultado.Exception?.MessagingErrorCode is { } codigo && ErroresDeToken.Contains(codigo))
                    invalidos.Add(lote[i]);
                else
                    errorTemporal ??= resultado.Exception?.Message ?? "Error desconocido de Firebase.";
            }
        }

        _logger.LogInformation(
            "[PUSH Firebase] \"{Titulo}\" | enviados: {Enviados}, inválidos: {Invalidos}",
            mensaje.Titulo, enviados, invalidos.Count);

        // Solo se reintenta si no llegó a nadie: si llegó a algún celular, reintentar lo duplicaría ahí
        var error = enviados == 0 ? errorTemporal : null;

        return new ResultadoPush(enviados, invalidos, error);
    }

    private static MulticastMessage Construir(MensajePush mensaje, IReadOnlyList<string> tokens) => new()
    {
        Tokens = tokens,
        Notification = new Notification
        {
            Title = mensaje.Titulo,
            Body = mensaje.Cuerpo
        },
        // Datos para que la app abra la pantalla correcta al tocar la notificación
        Data = mensaje.Datos,
        Android = new AndroidConfig
        {
            Priority = Priority.High
        },
        Apns = new ApnsConfig
        {
            Aps = new Aps { Sound = "default" }
        }
    };

    public void Dispose() => _app.Delete();
}
