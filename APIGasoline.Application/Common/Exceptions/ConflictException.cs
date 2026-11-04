namespace APIGasoline.Application.Common.Exceptions;

/// <summary>
/// 409. <see cref="Codigo"/> y <see cref="Datos"/> viajan en el ProblemDetails (extensiones "codigo" y los
/// nombres de <see cref="Datos"/>) para que la app pueda decidir qué hacer sin leer el texto del mensaje.
/// </summary>
public class ConflictException : Exception
{
    public ConflictException(string message) : base(message)
    {
    }

    public ConflictException(string message, string codigo, IReadOnlyDictionary<string, object?>? datos = null) : base(message)
    {
        Codigo = codigo;
        Datos = datos;
    }

    /// <summary>Código estable para la app (ver <see cref="CodigosConflicto"/>).</summary>
    public string? Codigo { get; }

    public IReadOnlyDictionary<string, object?>? Datos { get; }
}

/// <summary>Códigos de conflicto que la app móvil interpreta.</summary>
public static class CodigosConflicto
{
    /// <summary>El carro ya tiene OTRO viaje en curso. Datos: viajeId, empleadoOperadorId, fechaInicio.</summary>
    public const string ViajeEnCurso = "VIAJE_EN_CURSO";

    /// <summary>El UUID mandado por la app ya existe con otros datos (no es un reintento).</summary>
    public const string IdDuplicado = "ID_DUPLICADO";

    /// <summary>Ya hay una carga del mismo viaje en la misma fecha y hora.</summary>
    public const string CargaMismaFecha = "CARGA_MISMA_FECHA";
}
