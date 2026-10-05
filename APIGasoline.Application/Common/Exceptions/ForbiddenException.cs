namespace APIGasoline.Application.Common.Exceptions;

/// <summary>
/// Autenticado, pero sin permiso para continuar (HTTP 403). Ej. usuario inactivo.
/// </summary>
public class ForbiddenException : Exception
{
    public ForbiddenException(string message) : base(message)
    {
    }
}
