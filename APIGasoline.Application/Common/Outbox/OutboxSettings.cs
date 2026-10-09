namespace APIGasoline.Application.Common.Outbox;

/// <summary>Sección "Outbox" de appsettings (o variables de entorno Outbox__IntervaloSegundos, etc.).</summary>
public class OutboxSettings
{
    public const string SectionName = "Outbox";

    /// <summary>Permite apagar el worker (por ejemplo, en pruebas de carga).</summary>
    public bool Habilitado { get; set; } = true;

    /// <summary>Cada cuánto revisa si hay mensajes pendientes.</summary>
    public int IntervaloSegundos { get; set; } = 5;

    /// <summary>Cuántos mensajes toma por vuelta.</summary>
    public int TamanoLote { get; set; } = 20;

    /// <summary>Si el worker se cae a la mitad, después de este tiempo otro vuelve a tomar el mensaje.</summary>
    public int BloqueoSegundos { get; set; } = 120;

    public void Validar()
    {
        if (IntervaloSegundos is < 1 or > 3600)
            throw new InvalidOperationException("Outbox:IntervaloSegundos debe estar entre 1 y 3600.");

        if (TamanoLote is < 1 or > 500)
            throw new InvalidOperationException("Outbox:TamanoLote debe estar entre 1 y 500.");

        if (BloqueoSegundos is < 10 or > 3600)
            throw new InvalidOperationException("Outbox:BloqueoSegundos debe estar entre 10 y 3600.");
    }
}
