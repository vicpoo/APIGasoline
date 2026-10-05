using APIGasoline.Domain.Enums;
using APIGasoline.Domain.Exceptions;

namespace APIGasoline.Domain.Entities;

/// <summary>
/// Vehículo perteneciente a una empresa.
/// </summary>
public class Carro
{
    public const int MarcaMaxLength = 80;
    public const int ModeloMaxLength = 80;
    public const int ColorMaxLength = 50;
    public const int PlacasMaxLength = 20;
    public const int FotoMaxLength = 255;
    public const int AnioMinimo = 1900;
    public const decimal CapacidadTanqueMaxima = 9999.99m;      // decimal(6,2)
    public const decimal KilometrajeMaximo = 99999999.99m;      // decimal(10,2)

    public int CarroId { get; private set; }
    public int EmpresaId { get; private set; }
    public string Marca { get; private set; } = string.Empty;
    public string Modelo { get; private set; } = string.Empty;
    public int? Anio { get; private set; }
    public string? Color { get; private set; }
    public string Placas { get; private set; } = string.Empty;

    /// <summary>Ruta/llave del archivo en el almacenamiento (MinIO).</summary>
    public string? FotoVehiculo { get; private set; }

    /// <summary>Litros.</summary>
    public decimal? CapacidadTanque { get; private set; }

    /// <summary>Kilómetros. Solo puede aumentar.</summary>
    public decimal KilometrajeActual { get; private set; }

    public EstadoCarro Estado { get; private set; } = EstadoCarro.Disponible;

    /// <summary>
    /// La asigna la base de datos al insertar (default now()). No se modifica después.
    /// </summary>
    public DateTime? FechaAlta { get; private set; }

    // Requerido por Entity Framework
    private Carro()
    {
    }

    public static Carro Crear(
        int empresaId,
        string marca,
        string modelo,
        int? anio,
        string? color,
        string placas,
        string? fotoVehiculo,
        decimal? capacidadTanque,
        decimal kilometrajeActual,
        EstadoCarro estado = EstadoCarro.Disponible)
    {
        if (kilometrajeActual < 0 || kilometrajeActual > KilometrajeMaximo)
            throw new DomainException($"El kilometraje debe estar entre 0 y {KilometrajeMaximo}.");

        var carro = new Carro { KilometrajeActual = kilometrajeActual };
        carro.CambiarEmpresa(empresaId);
        carro.CambiarDatos(marca, modelo, anio, color);
        carro.CambiarPlacas(placas);
        carro.CambiarFotoVehiculo(fotoVehiculo);
        carro.CambiarCapacidadTanque(capacidadTanque);
        carro.CambiarEstado(estado);

        return carro;
    }

    public void CambiarEmpresa(int empresaId)
    {
        if (empresaId <= 0)
            throw new DomainException("El id de la empresa debe ser mayor a cero.");

        EmpresaId = empresaId;
    }

    public void CambiarDatos(string marca, string modelo, int? anio, string? color)
    {
        Marca = Requerido(marca, "La marca", MarcaMaxLength);
        Modelo = Requerido(modelo, "El modelo", ModeloMaxLength);

        var anioMaximo = DateTime.UtcNow.Year + 1;
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

    /// <summary>
    /// El kilometraje nunca puede disminuir (se actualiza al devolver el carro).
    /// </summary>
    public void ActualizarKilometraje(decimal nuevoKilometraje)
    {
        if (nuevoKilometraje > KilometrajeMaximo)
            throw new DomainException($"El kilometraje no puede exceder {KilometrajeMaximo}.");

        if (nuevoKilometraje < KilometrajeActual)
            throw new DomainException($"El kilometraje no puede ser menor al actual ({KilometrajeActual}).");

        KilometrajeActual = nuevoKilometraje;
    }

    /// <summary>
    /// Al cerrar un viaje: actualiza el kilometraje solo si es mayor al actual.
    /// (Los viajes sincronizados offline pueden llegar en desorden.)
    /// </summary>
    public void AvanzarKilometraje(decimal kilometraje)
    {
        if (kilometraje > KilometrajeMaximo)
            throw new DomainException($"El kilometraje no puede exceder {KilometrajeMaximo}.");

        if (kilometraje > KilometrajeActual)
            KilometrajeActual = kilometraje;
    }

    public void CambiarEstado(EstadoCarro estado)
    {
        if (!Enum.IsDefined(estado))
            throw new DomainException("El estado del carro no es válido.");

        Estado = estado;
    }

    public bool EstaDisponible => Estado == EstadoCarro.Disponible;

    /// <summary>
    /// Se llama al aprobar una solicitud de reserva.
    /// </summary>
    public void Ocupar()
    {
        if (Estado == EstadoCarro.EnMantenimiento)
            throw new DomainException("El carro está en mantenimiento y no se puede asignar.");

        if (Estado == EstadoCarro.Ocupado)
            throw new DomainException("El carro ya está ocupado.");

        Estado = EstadoCarro.Ocupado;
    }

    /// <summary>
    /// Se llama al cancelar una reserva aprobada (o al devolver el carro).
    /// </summary>
    public void Liberar()
    {
        if (Estado == EstadoCarro.Ocupado)
            Estado = EstadoCarro.Disponible;
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
