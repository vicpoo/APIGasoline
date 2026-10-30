using APIGasoline.Domain.Enums;
using APIGasoline.Domain.Exceptions;

namespace APIGasoline.Domain.Entities;

public class Carro
{
    public const int MarcaMaxLength = 80;
    public const int ModeloMaxLength = 80;
    public const int ColorMaxLength = 50;
    public const int PlacasMaxLength = 20;
    public const int FotoMaxLength = 255;
    public const int MotivoBajaMaxLength = 255;
    public const int AnioMinimo = 1900;
    public const decimal CapacidadTanqueMaxima = 9999.99m;      // decimal(6,2)
    public const decimal KilometrajeMaximo = 99999999.99m;      // decimal(10,2)

    public Guid CarroId { get; private set; } = Guid.CreateVersion7();
    public Guid EmpresaId { get; private set; }
    public string Marca { get; private set; } = string.Empty;
    public string Modelo { get; private set; } = string.Empty;
    public int? Anio { get; private set; }
    public string? Color { get; private set; }
    public string Placas { get; private set; } = string.Empty;

    public string? FotoVehiculo { get; private set; }

    public decimal? CapacidadTanque { get; private set; }

    public decimal KilometrajeActual { get; private set; }

    public EstadoCarro Estado { get; private set; } = EstadoCarro.Disponible;

    /// <summary>Carro, moto, camioneta o camión (la app muestra el ícono según el tipo).</summary>
    public TipoVehiculo TipoVehiculo { get; private set; } = TipoVehiculo.Carro;

    /// <summary>Baja del carro (HU-AST-03). El historial de viajes se conserva.</summary>
    public bool DadoDeBaja { get; private set; }
    public DateTime? FechaBaja { get; private set; }
    public string? MotivoBaja { get; private set; }

    public DateTime? FechaAlta { get; private set; }

    // Requerido por Entity Framework
    private Carro()
    {
    }

    public static Carro Crear(
        Guid empresaId,
        string marca,
        string modelo,
        int? anio,
        string? color,
        string placas,
        string? fotoVehiculo,
        decimal? capacidadTanque,
        decimal kilometrajeActual,
        int anioActual,
        EstadoCarro estado = EstadoCarro.Disponible,
        TipoVehiculo tipoVehiculo = TipoVehiculo.Carro)
    {
        if (kilometrajeActual < 0 || kilometrajeActual > KilometrajeMaximo)
            throw new DomainException($"El kilometraje debe estar entre 0 y {KilometrajeMaximo}.");

        var carro = new Carro { KilometrajeActual = kilometrajeActual };
        carro.CambiarEmpresa(empresaId);
        carro.CambiarDatos(marca, modelo, anio, color, anioActual);
        carro.CambiarPlacas(placas);
        carro.CambiarFotoVehiculo(fotoVehiculo);
        carro.CambiarCapacidadTanque(capacidadTanque);
        carro.CambiarEstado(estado);
        carro.CambiarTipoVehiculo(tipoVehiculo);

        return carro;
    }

    public void CambiarEmpresa(Guid empresaId)
    {
        if (empresaId == Guid.Empty)
            throw new DomainException("El id de la empresa es obligatorio.");

        EmpresaId = empresaId;
    }

    /// <param name="anioActual">
    /// Año de hoy (lo da IDateTimeProvider). Se recibe como parámetro para que la regla
    /// no dependa del reloj del sistema y se pueda probar.
    /// </param>
    public void CambiarDatos(string marca, string modelo, int? anio, string? color, int anioActual)
    {
        Marca = Requerido(marca, "La marca", MarcaMaxLength);
        Modelo = Requerido(modelo, "El modelo", ModeloMaxLength);

        // Se permite el modelo del año siguiente (los autos nuevos salen antes)
        var anioMaximo = anioActual + 1;
        if (anio is not null && (anio < AnioMinimo || anio > anioMaximo))
            throw new DomainException($"El año debe estar entre {AnioMinimo} y {anioMaximo}.");

        Anio = anio;
        Color = Opcional(color, "El color", ColorMaxLength);
    }

    public void CambiarPlacas(string placas)
    {
        // Se guardan en mayúsculas para que la validación de únicas no dependa del formato
        Placas = Requerido(placas, "Las placas", PlacasMaxLength).ToUpperInvariant();
    }

    public void CambiarFotoVehiculo(string? fotoVehiculo)
        => FotoVehiculo = Opcional(fotoVehiculo, "La ruta de la foto", FotoMaxLength);

    public void CambiarCapacidadTanque(decimal? capacidadTanque)
    {
        if (capacidadTanque is not null && (capacidadTanque <= 0 || capacidadTanque > CapacidadTanqueMaxima))
            throw new DomainException($"La capacidad del tanque debe ser mayor a 0 y menor o igual a {CapacidadTanqueMaxima}.");

        CapacidadTanque = capacidadTanque;
    }


    public void ActualizarKilometraje(decimal nuevoKilometraje)
    {
        if (nuevoKilometraje > KilometrajeMaximo)
            throw new DomainException($"El kilometraje no puede exceder {KilometrajeMaximo}.");

        if (nuevoKilometraje < KilometrajeActual)
            throw new DomainException($"El kilometraje no puede ser menor al actual ({KilometrajeActual}).");

        KilometrajeActual = nuevoKilometraje;
    }


    public void AvanzarKilometraje(decimal kilometraje)
    {
        if (kilometraje > KilometrajeMaximo)
            throw new DomainException($"El kilometraje no puede exceder {KilometrajeMaximo}.");

        if (kilometraje > KilometrajeActual)
            KilometrajeActual = kilometraje;
    }

    /// <summary>
    /// Ajusta el kilometraje cuando se corrige el último viaje del carro (puede bajar).
    /// Solo lo usa el flujo de corrección; el uso normal solo avanza el kilometraje.
    /// </summary>
    public void CorregirKilometraje(decimal kilometraje)
    {
        if (kilometraje < 0 || kilometraje > KilometrajeMaximo)
            throw new DomainException($"El kilometraje debe estar entre 0 y {KilometrajeMaximo}.");

        KilometrajeActual = kilometraje;
    }

    public void CambiarTipoVehiculo(TipoVehiculo tipoVehiculo)
    {
        if (!Enum.IsDefined(tipoVehiculo))
            throw new DomainException("El tipo de vehículo no es válido.");

        TipoVehiculo = tipoVehiculo;
    }

    public void CambiarEstado(EstadoCarro estado)
    {
        if (!Enum.IsDefined(estado))
            throw new DomainException("El estado del carro no es válido.");

        Estado = estado;
    }

    public bool EstaDisponible => Estado == EstadoCarro.Disponible && !DadoDeBaja;

 
    public void Ocupar()
    {
        if (DadoDeBaja)
            throw new DomainException("El carro está dado de baja y no se puede asignar.");

        if (Estado == EstadoCarro.EnMantenimiento)
            throw new DomainException("El carro está en mantenimiento y no se puede asignar.");

        if (Estado == EstadoCarro.Ocupado)
            throw new DomainException("El carro ya está ocupado.");

        Estado = EstadoCarro.Ocupado;
    }

 
    public void Liberar()
    {
        if (Estado == EstadoCarro.Ocupado)
            Estado = EstadoCarro.Disponible;
    }

    /// <summary>
    /// Retira el carro de la flota (HU-AST-03). No se puede dar de baja un carro en uso.
    /// </summary>
    public void DarDeBaja(DateTime fecha, string motivo)
    {
        if (DadoDeBaja)
            throw new DomainException("El carro ya está dado de baja.");

        if (Estado == EstadoCarro.Ocupado)
            throw new DomainException("No se puede dar de baja un carro que está en uso. Primero libéralo.");

        if (fecha == default)
            throw new DomainException("La fecha de baja es obligatoria.");

        MotivoBaja = Requerido(motivo, "El motivo de baja", MotivoBajaMaxLength);
        FechaBaja = fecha;
        DadoDeBaja = true;
    }

    private static string Requerido(string? valor, string campo, int max)
    {
        var normalizado = valor?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(normalizado))
            throw new DomainException($"{campo} es obligatorio.");

        if (normalizado.Length > max)
            throw new DomainException($"{campo} no puede exceder {max} caracteres.");

        return normalizado;
    }

    private static string? Opcional(string? valor, string campo, int max)
    {
        var normalizado = string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();

        if (normalizado is not null && normalizado.Length > max)
            throw new DomainException($"{campo} no puede exceder {max} caracteres.");

        return normalizado;
    }
}
