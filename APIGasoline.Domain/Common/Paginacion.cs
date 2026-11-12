namespace APIGasoline.Domain.Common;

/// <summary>
/// Qué página de un listado se pide (F011). <see cref="Pagina"/> empieza en 1. El tamaño se acota a
/// <see cref="TamanoMaximo"/> (o al máximo propio del listado): así ningún listado devuelve todo el
/// historial de una vez.
/// </summary>
public sealed record Paginacion
{
    public const int TamanoPorDefecto = 50;
    public const int TamanoMaximo = 200;

    public int Pagina { get; }
    public int TamanoPagina { get; }

    public Paginacion(int? pagina = null, int? tamanoPagina = null, int porDefecto = TamanoPorDefecto, int maximo = TamanoMaximo)
    {
        Pagina = Math.Max(1, pagina ?? 1);
        TamanoPagina = Math.Clamp(tamanoPagina ?? porDefecto, 1, maximo);
    }

    /// <summary>Filas que se saltan antes de esta página.</summary>
    public int Saltar => (Pagina - 1) * TamanoPagina;

    public static Paginacion PorDefecto { get; } = new();
}

/// <summary>Una página de resultados y cuántos hay en total con esos filtros.</summary>
public sealed record PaginaDe<T>(IReadOnlyList<T> Elementos, int Total, Paginacion Paginacion)
{
    /// <summary>Misma página con otros elementos (por ejemplo, ya convertidos a DTO).</summary>
    public PaginaDe<TOtro> Con<TOtro>(IReadOnlyList<TOtro> elementos) => new(elementos, Total, Paginacion);
}
