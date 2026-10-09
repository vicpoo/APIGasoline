using APIGasoline.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace APIGasoline.Infrastructure.Push;

/// <summary>
/// Envío simulado (sin Firebase): solo escribe en el log. Sirve para desarrollo y pruebas.
/// Para probar los casos de error desde Postman:
/// - un token que empieza con "invalido" se reporta como inválido (el dispositivo se desactiva);
/// - un token que empieza con "error" simula que el servicio está caído (el mensaje se reintenta).
/// </summary>
public class NoOpPushService : IPushNotificationService
{
    private readonly ILogger<NoOpPushService> _logger;

    public NoOpPushService(ILogger<NoOpPushService> logger)
    {
        _logger = logger;
    }

    public Task<ResultadoPush> EnviarAsync(MensajePush mensaje, CancellationToken cancellationToken = default)
    {
        if (mensaje.Tokens.Any(t => t.StartsWith("error", StringComparison.OrdinalIgnoreCase)))
            return Task.FromResult(ResultadoPush.Fallo("Servicio de push no disponible (simulado)."));

        var invalidos = mensaje.Tokens
            .Where(t => t.StartsWith("invalido", StringComparison.OrdinalIgnoreCase))
            .ToList();

        var enviados = mensaje.Tokens.Count - invalidos.Count;

        _logger.LogInformation(
            "[PUSH simulado] \"{Titulo}\" - {Cuerpo} | enviados: {Enviados}, inválidos: {Invalidos}",
            mensaje.Titulo, mensaje.Cuerpo, enviados, invalidos.Count);

        return Task.FromResult(new ResultadoPush(enviados, invalidos, null));
    }
}
