namespace APIGasoline.Application.Common.Exceptions;

/// <summary>
/// Datos de entrada inválidos que dependen de otras entidades
/// (por ejemplo: la división no pertenece al corporativo indicado).
/// </summary>
public class BadRequestException : Exception
{
    public BadRequestException(string message) : base(message)
    {
    }
}
