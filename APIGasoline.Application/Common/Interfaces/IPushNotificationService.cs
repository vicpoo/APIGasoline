namespace APIGasoline.Application.Common.Interfaces;

public record MensajePush(
    IReadOnlyList<string> Tokens,
    string Titulo,
    string Cuerpo,
    IReadOnlyDictionary<string, string> Datos);

/// <param name="Enviados">Tokens a los que sí llegó el push.</param>
/// <param name="TokensInvalidos">Tokens que ya no existen (app desinstalada, sesión cerrada): se desactivan.</param>
/// <param name="Error">Falla general que vale la pena reintentar (servicio caído, sin red). Null si no hubo.</param>
public record ResultadoPush(int Enviados, IReadOnlyList<string> TokensInvalidos, string? Error)
{
    public static ResultadoPush Fallo(string error) => new(0, [], error);
}

public interface IPushNotificationService
{
    Task<ResultadoPush> EnviarAsync(MensajePush mensaje, CancellationToken cancellationToken = default);
}
