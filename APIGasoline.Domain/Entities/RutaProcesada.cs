using APIGasoline.Domain.Exceptions;
using APIGasoline.Domain.Rutas;

namespace APIGasoline.Domain.Entities;


public class RutaProcesada
{
    public int RutaProcesadaId { get; private set; }
    public int EmpresaId { get; private set; }
    public int ViajeId { get; private set; }

    // --- Limpieza ---
    public int PuntosRecibidos { get; private set; }
    public int PuntosDescartados { get; private set; }

    // --- Distancia y tiempos según el GPS ---
    public decimal DistanciaGpsKm { get; private set; }
    public int TiempoMovimientoSeg { get; private set; }
    public int TiempoDetenidoSeg { get; private set; }
    public decimal? VelocidadPromedioKmh { get; private set; }
    public decimal? VelocidadMaximaKmh { get; private set; }

    // --- GPS contra odómetro ---
    public decimal? DistanciaOdometroKm { get; private set; }
    public decimal? DiferenciaKm { get; private set; }
    public decimal? DiferenciaPorcentaje { get; private set; }
    public bool AlertaKilometraje { get; private set; }

    // --- Combustible ---
    public decimal LitrosCargados { get; private set; }
    public decimal? RendimientoKmPorLitro { get; private set; }

    // --- Mapa ---
    public string? Polyline { get; private set; }

    // --- Control ---
    public string VersionAlgoritmo { get; private set; } = string.Empty;
    public DateTime FechaProceso { get; private set; }

    // Requerido por Entity Framework
    private RutaProcesada()
    {
    }

    public static RutaProcesada Crear(int empresaId, int viajeId, ResultadoRuta resultado, DateTime fechaProceso)
    {
        if (empresaId <= 0)
            throw new DomainException("El id de la empresa debe ser mayor a cero.");

        if (viajeId <= 0)
            throw new DomainException("El id del viaje debe ser mayor a cero.");

        var ruta = new RutaProcesada { EmpresaId = empresaId, ViajeId = viajeId };
        ruta.Actualizar(resultado, fechaProceso);

        return ruta;
    }

    /// Reemplaza el resumen con un nuevo cálculo (por ejemplo, al mejorar el algoritmo).
    /// Los puntos crudos no cambian; solo se recalcula lo derivado.
     

    public void Actualizar(ResultadoRuta resultado, DateTime fechaProceso)
    {
        ArgumentNullException.ThrowIfNull(resultado);

        if (fechaProceso == default)
            throw new DomainException("La fecha de proceso es obligatoria.");

        if (string.IsNullOrWhiteSpace(resultado.VersionAlgoritmo))
            throw new DomainException("La versión del algoritmo es obligatoria.");

        PuntosRecibidos = resultado.PuntosRecibidos;
        PuntosDescartados = resultado.PuntosDescartados;
        DistanciaGpsKm = resultado.DistanciaGpsKm;
        TiempoMovimientoSeg = resultado.TiempoMovimientoSeg;
        TiempoDetenidoSeg = resultado.TiempoDetenidoSeg;
        VelocidadPromedioKmh = resultado.VelocidadPromedioKmh;
        VelocidadMaximaKmh = resultado.VelocidadMaximaKmh;
        DistanciaOdometroKm = resultado.Odometro.DistanciaOdometroKm;
        DiferenciaKm = resultado.Odometro.DiferenciaKm;
        DiferenciaPorcentaje = resultado.Odometro.DiferenciaPorcentaje;
        AlertaKilometraje = resultado.Odometro.Alerta;
        LitrosCargados = resultado.LitrosCargados;
        RendimientoKmPorLitro = resultado.RendimientoKmPorLitro;
        Polyline = string.IsNullOrEmpty(resultado.Polyline) ? null : resultado.Polyline;
        VersionAlgoritmo = resultado.VersionAlgoritmo;
        FechaProceso = fechaProceso;
    }
}
