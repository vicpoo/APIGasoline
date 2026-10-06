namespace APIGasoline.Application.Common.Exceptions;

/// <summary>
/// El servicio de almacenamiento de archivos (MinIO) no está disponible o falló.
/// </summary>
public class AlmacenamientoException : Exception
{
    public AlmacenamientoException(string message, Exception? innerException = null) : base(message, innerException)
    {
    }
}
