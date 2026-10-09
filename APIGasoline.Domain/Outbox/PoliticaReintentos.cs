namespace APIGasoline.Domain.Outbox;

/// <summary>
/// 5 intentos en total: el primero y 4 reintentos con espera creciente.
/// Si el quinto también falla, el mensaje queda como fallido.
/// </summary>
public static class PoliticaReintentos
{
    public const int MaxIntentos = 5;

    private static readonly TimeSpan[] Esperas =
    [
        TimeSpan.FromSeconds(30),
        TimeSpan.FromMinutes(2),
        TimeSpan.FromMinutes(10),
        TimeSpan.FromMinutes(30)
    ];

    /// <summary>Espera antes del siguiente intento, según cuántos intentos ya fallaron.</summary>
    public static TimeSpan EsperaTras(int intentosFallidos)
    {
        if (intentosFallidos < 1)
            throw new ArgumentOutOfRangeException(nameof(intentosFallidos), "Debe haber al menos un intento fallido.");

        return Esperas[Math.Min(intentosFallidos, Esperas.Length) - 1];
    }

    public static bool SeAgotaron(int intentosFallidos) => intentosFallidos >= MaxIntentos;
}
