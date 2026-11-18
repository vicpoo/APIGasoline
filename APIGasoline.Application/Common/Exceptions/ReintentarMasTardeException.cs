namespace APIGasoline.Application.Common.Exceptions;

/// <summary>
/// La operación chocó con otra que corría al mismo tiempo y no se pudo completar después de
/// reintentarla. Los datos no tienen nada mal: el cliente debe volver a mandarla en un momento.
/// Se responde 503 con Retry-After (la app lo trata como error temporal y reintenta sola;
/// un 409 haría que descartara el lote de puntos GPS).
/// </summary>
public sealed class ReintentarMasTardeException : Exception
{
    public ReintentarMasTardeException(string message, Exception? innerException = null) : base(message, innerException)
    {
    }
}
