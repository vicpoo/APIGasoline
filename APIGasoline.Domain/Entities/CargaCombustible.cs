using APIGasoline.Domain.Exceptions;

namespace APIGasoline.Domain.Entities;

/// <summary>
/// Registra una carga de combustible, incluyendo litros, monto, comprobante y ubicación geográfica.
/// </summary>
public class CargaCombustible
{
    public const decimal LitrosMaximo = 999999.99m;     // decimal(8,2)
    public const decimal MontoMaximo = 99999999.99m;    // decimal(10,2)
    public const int FotoMaxLength = 255;

    public int CargaCombustibleId { get; private set; }
    public int EmpresaId { get; private set; }
    public int ViajeId { get; private set; }
    public decimal Litros { get; private set; }
    public decimal? Monto { get; private set; }

    /// <summary>Ruta/llave del comprobante en el almacenamiento (MinIO).</summary>
    public string FotoComprobante { get; private set; } = string.Empty;

    /// <summary>La manda la app (la carga pudo registrarse offline).</summary>
    public DateTime FechaCarga { get; private set; }

    public decimal? UbicacionLat { get; private set; }
    public decimal? UbicacionLng { get; private set; }

    /// <summary>true cuando la carga se registró sin conexión y se subió después.</summary>
    public bool Sincronizado { get; private set; }

    // Requerido por Entity Framework
    private CargaCombustible()
    {
    }

    public static CargaCombustible Crear(
        int empresaId,
        int viajeId,
        decimal litros,
        decimal? monto,
        string fotoComprobante,
        DateTime fechaCarga,
        decimal? lat,
        decimal? lng,
        bool sincronizado)
    {
        if (empresaId <= 0)
            throw new DomainException("El id de la empresa debe ser mayor a cero.");

        if (viajeId <= 0)
            throw new DomainException("El id del viaje debe ser mayor a cero.");

        var carga = new CargaCombustible
        {
            EmpresaId = empresaId,
            ViajeId = viajeId,
            Sincronizado = sincronizado
        };

        carga.CambiarDatos(litros, monto, fotoComprobante, fechaCarga, lat, lng);

        return carga;
    }

    public void CambiarDatos(decimal litros, decimal? monto, string fotoComprobante, DateTime fechaCarga, decimal? lat, decimal? lng)
    {
        if (litros <= 0 || litros > LitrosMaximo)
            throw new DomainException($"Los litros deben ser mayores a 0 y menores o iguales a {LitrosMaximo}.");

        if (monto is not null && (monto < 0 || monto > MontoMaximo))
            throw new DomainException($"El monto debe estar entre 0 y {MontoMaximo}.");

        var foto = fotoComprobante?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(foto))
            throw new DomainException("La foto del comprobante es obligatoria.");

        if (foto.Length > FotoMaxLength)
            throw new DomainException($"La ruta del comprobante no puede exceder {FotoMaxLength} caracteres.");

        if (fechaCarga == default)
            throw new DomainException("La fecha de carga es obligatoria.");

        if ((lat is null) != (lng is null))
            throw new DomainException("La ubicación debe incluir latitud y longitud juntas.");

        if (lat is < -90m or > 90m)
            throw new DomainException("La latitud debe estar entre -90 y 90.");

        if (lng is < -180m or > 180m)
            throw new DomainException("La longitud debe estar entre -180 y 180.");

        Litros = litros;
        Monto = monto;
        FotoComprobante = foto;
        // La columna timestamp guarda segundos; se trunca para comparar igual que en la BD
        FechaCarga = new DateTime(fechaCarga.Ticks - (fechaCarga.Ticks % TimeSpan.TicksPerSecond), fechaCarga.Kind);
        UbicacionLat = lat;
        UbicacionLng = lng;
    }

    /// <summary>Precio por litro calculado (si hay monto).</summary>
    public decimal? PrecioPorLitro => Monto is null ? null : Math.Round(Monto.Value / Litros, 2);
}
