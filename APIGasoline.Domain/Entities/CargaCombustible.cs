using APIGasoline.Domain.Exceptions;

namespace APIGasoline.Domain.Entities;

public class CargaCombustible
{
    public const decimal LitrosMaximo = 999999.99m;     // decimal(8,2)
    public const decimal MontoMaximo = 99999999.99m;    // decimal(10,2)
    public const int FotoMaxLength = 255;

    public Guid CargaCombustibleId { get; private set; } = Guid.CreateVersion7();
    public Guid EmpresaId { get; private set; }
    public Guid ViajeId { get; private set; }
    public decimal Litros { get; private set; }
    public decimal? Monto { get; private set; }

    public string FotoComprobante { get; private set; } = string.Empty;

    public DateTime FechaCarga { get; private set; }

    public decimal? UbicacionLat { get; private set; }
    public decimal? UbicacionLng { get; private set; }

    public bool Sincronizado { get; private set; }

    // Requerido por Entity Framework
    private CargaCombustible()
    {
    }

    public static CargaCombustible Crear(
        Guid empresaId,
        Guid viajeId,
        decimal litros,
        decimal? monto,
        string fotoComprobante,
        DateTime fechaCarga,
        decimal? lat,
        decimal? lng,
        bool sincronizado)
    {
        if (empresaId == Guid.Empty)
            throw new DomainException("El id de la empresa es obligatorio.");

        if (viajeId == Guid.Empty)
            throw new DomainException("El id del viaje es obligatorio.");

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

    public decimal? PrecioPorLitro => Monto is null ? null : Math.Round(Monto.Value / Litros, 2);
}
