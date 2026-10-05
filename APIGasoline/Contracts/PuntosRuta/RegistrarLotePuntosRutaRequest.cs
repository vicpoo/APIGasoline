using System.ComponentModel.DataAnnotations;

namespace APIGasoline.Contracts.PuntosRuta;

public class RegistrarLotePuntosRutaRequest
{
    /// <summary>true si los puntos se capturaron sin conexión.</summary>
    public bool Sincronizado { get; set; }

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
}
