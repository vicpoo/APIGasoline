using APIGasoline.Domain.Enums;
using APIGasoline.Domain.Exceptions;

namespace APIGasoline.Domain.Entities;


public class Viaje
{
    public const int MotivoMaxLength = 255;
    public const decimal KilometrajeMaximo = 99999999.99m; // decimal(10,2)

    public int ViajeId { get; private set; }
    public int EmpresaId { get; private set; }
    public int CarroId { get; private set; }
    public int? SolicitudReservaId { get; private set; }
    public int EmpleadoOperadorId { get; private set; }

    public DateTime FechaInicio { get; private set; }
    public decimal? UbicacionInicioLat { get; private set; }
    public decimal? UbicacionInicioLng { get; private set; }
    public decimal KilometrajeInicial { get; private set; }

    public DateTime? FechaFin { get; private set; }
    public decimal? UbicacionFinLat { get; private set; }
    public decimal? UbicacionFinLng { get; private set; }
    public decimal? KilometrajeFinal { get; private set; }

    /// Se calcula: kilometraje final - kilometraje inicial.
    public decimal? DistanciaRecorrida { get; private set; }

    public EstadoViaje Estado { get; private set; } = EstadoViaje.EnCurso;
    public string? MotivoInterrupcion { get; private set; }

    /// true cuando el viaje se registró desde la app en modo offline y se subió después.

    public bool Sincronizado { get; private set; }

    // Requerido por Entity Framework
    private Viaje()
    {
    }

    public static Viaje Iniciar(
        int empresaId,
        int carroId,
        int? solicitudReservaId,
        int empleadoOperadorId,
        DateTime fechaInicio,
        decimal? lat,
        decimal? lng,
        decimal kilometrajeInicial)
    {
        if (empresaId <= 0)
            throw new DomainException("El id de la empresa debe ser mayor a cero.");

        if (carroId <= 0)
            throw new DomainException("El id del carro debe ser mayor a cero.");

        if (solicitudReservaId is <= 0)
            throw new DomainException("El id de la solicitud de reserva debe ser mayor a cero.");

        if (empleadoOperadorId <= 0)
            throw new DomainException("El id del empleado operador debe ser mayor a cero.");

        if (fechaInicio == default)
            throw new DomainException("La fecha de inicio es obligatoria.");

        ValidarKilometraje(kilometrajeInicial, "inicial");
        ValidarUbicacion(lat, lng, "de inicio");

        return new Viaje
        {
            EmpresaId = empresaId,
            CarroId = carroId,
            SolicitudReservaId = solicitudReservaId,
            EmpleadoOperadorId = empleadoOperadorId,
            FechaInicio = fechaInicio,
            UbicacionInicioLat = lat,
            UbicacionInicioLng = lng,
            KilometrajeInicial = kilometrajeInicial,
            Estado = EstadoViaje.EnCurso
        };
    }

    public bool EstaEnCurso => Estado == EstadoViaje.EnCurso;

    public void Finalizar(DateTime fechaFin, decimal? lat, decimal? lng, decimal kilometrajeFinal)
    {
        ValidarEnCurso("finalizar");
        RegistrarCierre(fechaFin, lat, lng, kilometrajeFinal);

        Estado = EstadoViaje.Finalizado;
    }

    /// El viaje se corta antes de terminar (falla, accidente, etc.). El motivo es obligatorio;
    /// los datos de cierre son opcionales porque puede que no se puedan capturar.

    public void Interrumpir(string motivo, DateTime fechaFin, decimal? lat, decimal? lng, decimal? kilometrajeFinal)
    {
        ValidarEnCurso("interrumpir");

        var motivoNormalizado = string.IsNullOrWhiteSpace(motivo) ? null : motivo.Trim();

        if (motivoNormalizado is null)
            throw new DomainException("El motivo es obligatorio para interrumpir un viaje.");

        if (motivoNormalizado.Length > MotivoMaxLength)
            throw new DomainException($"El motivo no puede exceder {MotivoMaxLength} caracteres.");

        RegistrarCierre(fechaFin, lat, lng, kilometrajeFinal);

        Estado = EstadoViaje.Interrumpido;
        MotivoInterrupcion = motivoNormalizado;
    }

    public void MarcarSincronizado() => Sincronizado = true;

    private void RegistrarCierre(DateTime fechaFin, decimal? lat, decimal? lng, decimal? kilometrajeFinal)
    {
        if (fechaFin < FechaInicio)
            throw new DomainException("La fecha de fin no puede ser anterior a la fecha de inicio.");

        ValidarUbicacion(lat, lng, "de fin");

        if (kilometrajeFinal is not null)
        {
            ValidarKilometraje(kilometrajeFinal.Value, "final");

            if (kilometrajeFinal < KilometrajeInicial)
                throw new DomainException($"El kilometraje final no puede ser menor al inicial ({KilometrajeInicial}).");

            DistanciaRecorrida = kilometrajeFinal - KilometrajeInicial;
        }

        FechaFin = fechaFin;
        UbicacionFinLat = lat;
        UbicacionFinLng = lng;
        KilometrajeFinal = kilometrajeFinal;
    }

    private void ValidarEnCurso(string accion)
    {
        if (!EstaEnCurso)
            throw new DomainException($"Solo se puede {accion} un viaje en curso (estado actual: {Estado}).");
    }

    private static void ValidarKilometraje(decimal kilometraje, string cual)
    {
        if (kilometraje < 0 || kilometraje > KilometrajeMaximo)
            throw new DomainException($"El kilometraje {cual} debe estar entre 0 y {KilometrajeMaximo}.");
    }

    private static void ValidarUbicacion(decimal? lat, decimal? lng, string cual)
    {
        if ((lat is null) != (lng is null))
            throw new DomainException($"La ubicación {cual} debe incluir latitud y longitud juntas.");

        if (lat is < -90m or > 90m)
            throw new DomainException($"La latitud {cual} debe estar entre -90 y 90.");

        if (lng is < -180m or > 180m)
            throw new DomainException($"La longitud {cual} debe estar entre -180 y 180.");
    }
}
