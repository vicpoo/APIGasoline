namespace APIGasoline.Application.Common.Exceptions;

/// <summary>
/// Credenciales inválidas (HTTP 401).
/// </summary>
public class UnauthorizedException : Exception
{
    public UnauthorizedException(string message) : base(message)
    {
    }
}
