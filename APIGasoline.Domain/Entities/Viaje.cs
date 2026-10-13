using APIGasoline.Domain.Enums;
using APIGasoline.Domain.Exceptions;

namespace APIGasoline.Domain.Entities;


public class Viaje
{
    public const int MotivoMaxLength = 255;
    public const int FotoMaxLength = 255;
    public const decimal KilometrajeMaximo = 99999999.99m; // decimal(10,2)

    public Guid ViajeId { get; private set; } = Guid.CreateVersion7();
    public Guid EmpresaId { get; private set; }
    public Guid CarroId { get; private set; }
    public Guid? SolicitudReservaId { get; private set; }
    public Guid EmpleadoOperadorId { get; private set; }

    public DateTime FechaInicio { get; private set; }
    public decimal? UbicacionInicioLat { get; private set; }
    public decimal? UbicacionInicioLng { get; private set; }
    public decimal KilometrajeInicial { get; private set; }

    /// <summary>Llave en MinIO de la foto del odómetro al iniciar.</summary>
    public string? FotoTableroInicial { get; private set; }

    public DateTime? FechaFin { get; private set; }
    public decimal? UbicacionFinLat { get; private set; }
    public decimal? UbicacionFinLng { get; private set; }
    public decimal? KilometrajeFinal { get; private set; }

    /// <summary>Llave en MinIO de la foto del odómetro al terminar.</summary>
    public string? FotoTableroFinal { get; private set; }

    /// Se calcula: kilometraje final - kilometraje inicial.
    public decimal? DistanciaRecorrida { get; private set; }

    public EstadoViaje Estado { get; private set; } = EstadoViaje.EnCurso;
    public string? MotivoInterrupcion { get; private set; }

    /// <summary>true si el kilometraje se corrigió después de capturarse. El detalle está en auditoria_evento.</summary>
    public bool Corregido { get; private set; }

    /// <summary>
    /// Hora del servidor en que llegó el viaje (la pone la BD). Comparada con <see cref="FechaInicio"/>
    /// indica cuánto tiempo estuvo sin sincronizar. Reemplaza al antiguo campo "sincronizado".
    /// </summary>
    public DateTime? FechaRecepcion { get; private set; }

    // Requerido por Entity Framework
    private Viaje()
    {
    }

    public static Viaje Iniciar(
        Guid empresaId,
        Guid carroId,
        Guid? solicitudReservaId,
        Guid empleadoOperadorId,
        DateTime fechaInicio,
        decimal? lat,
        decimal? lng,
        decimal kilometrajeInicial)
    {
        if (empresaId == Guid.Empty)
            throw new DomainException("El id de la empresa es obligatorio.");

        if (carroId == Guid.Empty)
            throw new DomainException("El id del carro es obligatorio.");

        if (solicitudReservaId == Guid.Empty)
            throw new DomainException("El id de la solicitud de reserva es obligatorio.");

        if (empleadoOperadorId == Guid.Empty)
            throw new DomainException("El id del empleado operador es obligatorio.");

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

    public void CambiarFotoTableroInicial(string? llave)
        => FotoTableroInicial = NormalizarFoto(llave, "inicial");

    public void CambiarFotoTableroFinal(string? llave)
        => FotoTableroFinal = NormalizarFoto(llave, "final");

    /// <summary>
    /// Corrige el kilometraje capturado por error. Recalcula la distancia y marca el viaje como corregido.
    /// Quién puede corregir (operador con viaje en curso o confirmador/gestor con viaje cerrado)
    /// lo decide la capa de aplicación.
    /// </summary>
    public void CorregirKilometraje(decimal kilometrajeInicial, decimal? kilometrajeFinal)
    {
        ValidarKilometraje(kilometrajeInicial, "inicial");

        if (kilometrajeFinal is not null)
        {
            if (EstaEnCurso)
                throw new DomainException("Un viaje en curso todavía no tiene kilometraje final.");

            ValidarKilometraje(kilometrajeFinal.Value, "final");

            if (kilometrajeFinal < kilometrajeInicial)
                throw new DomainException($"El kilometraje final no puede ser menor al inicial ({kilometrajeInicial}).");
        }
        else if (KilometrajeFinal is not null)
        {
            throw new DomainException("El kilometraje final no se puede borrar; indica el valor correcto.");
        }

        if (kilometrajeInicial == KilometrajeInicial && kilometrajeFinal == KilometrajeFinal)
            throw new DomainException("No hay cambios que corregir.");

        KilometrajeInicial = kilometrajeInicial;
        KilometrajeFinal = kilometrajeFinal;
        DistanciaRecorrida = kilometrajeFinal is null ? null : kilometrajeFinal - kilometrajeInicial;
        Corregido = true;
    }

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

    private static string? NormalizarFoto(string? llave, string cual)
    {
        var normalizada = string.IsNullOrWhiteSpace(llave) ? null : llave.Trim();

        if (normalizada is not null && normalizada.Length > FotoMaxLength)
            throw new DomainException($"La ruta de la foto del tablero {cual} no puede exceder {FotoMaxLength} caracteres.");

        return normalizada;
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
