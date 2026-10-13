using System.ComponentModel.DataAnnotations;

namespace APIGasoline.Contracts.PuntosRuta;

public class RegistrarLotePuntosRutaRequest
{
    [Required]
    [MinLength(1)]
    [MaxLength(1000)]
    public List<PuntoRutaRequest> Puntos { get; set; } = new();
}

public class PuntoRutaRequest
{
    [Range(-90, 90)]
    public decimal Latitud { get; set; }

    [Range(-180, 180)]
    public decimal Longitud { get; set; }

    [Required]
    public DateTime? TimestampCaptura { get; set; }

    /// <summary>Radio de error del GPS en metros (opcional). Con más de 50 m el punto se descarta al procesar.</summary>
    [Range(0, 9999.99)]
    public decimal? PrecisionMetros { get; set; }

    /// <summary>Metros sobre el nivel del mar (opcional).</summary>
    [Range(-500, 9000)]
    public decimal? Altitud { get; set; }

    /// <summary>Velocidad instantánea del GPS en m/s (opcional).</summary>
    [Range(0, 9999.99)]
    public decimal? Velocidad { get; set; }
}
