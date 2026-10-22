namespace APIGasoline.Contracts.Viajes;

public class ReanudarViajeRequest
{
    /// <summary>Hora en que se reanudó el viaje. Si no se manda, se usa la hora actual del servidor.</summary>
    public DateTime? Fecha { get; set; }
}
