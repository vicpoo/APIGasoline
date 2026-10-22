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

    /// <summary>Centro de costos al que se carga el viaje (opcional). Debe ser de la misma empresa.</summary>
    public Guid? CentroCostoId { get; private set; }

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
        decimal kilometrajeInicial,
        Guid? centroCostoId = null)
    {
        if (centroCostoId == Guid.Empty)
            throw new DomainException("El id del centro de costo no es válido.");

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
            CentroCostoId = centroCostoId,
            FechaInicio = fechaInicio,
            UbicacionInicioLat = lat,
            UbicacionInicioLng = lng,
            KilometrajeInicial = kilometrajeInicial,
            Estado = EstadoViaje.EnCurso
        };
    }

    public bool EstaEnCurso => Estado == EstadoViaje.EnCurso;

    /// <param name="pausaAbierta">Si el viaje estaba en pausa, la pausa se cierra a la hora de fin.</param>
    public void Finalizar(DateTime fechaFin, decimal? lat, decimal? lng, decimal kilometrajeFinal, ViajePausa? pausaAbierta = null)
    {
        ValidarEnCurso("finalizar");
        ValidarPausaAlTerminar(pausaAbierta, fechaFin);
        RegistrarCierre(fechaFin, lat, lng, kilometrajeFinal);
        CerrarPausaAlTerminar(pausaAbierta, fechaFin);

        Estado = EstadoViaje.Finalizado;
    }

    /// El viaje se corta antes de terminar (falla, accidente, etc.). El motivo es obligatorio;
    /// los datos de cierre son opcionales porque puede que no se puedan capturar.

    public void Interrumpir(string motivo, DateTime fechaFin, decimal? lat, decimal? lng, decimal? kilometrajeFinal, ViajePausa? pausaAbierta = null)
    {
        ValidarEnCurso("interrumpir");

        var motivoNormalizado = string.IsNullOrWhiteSpace(motivo) ? null : motivo.Trim();

        if (motivoNormalizado is null)
            throw new DomainException("El motivo es obligatorio para interrumpir un viaje.");

        if (motivoNormalizado.Length > MotivoMaxLength)
            throw new DomainException($"El motivo no puede exceder {MotivoMaxLength} caracteres.");

        ValidarPausaAlTerminar(pausaAbierta, fechaFin);
        RegistrarCierre(fechaFin, lat, lng, kilometrajeFinal);
        CerrarPausaAlTerminar(pausaAbierta, fechaFin);

        Estado = EstadoViaje.Interrumpido;
        MotivoInterrupcion = motivoNormalizado;
    }

    // ---------------- Pausas ----------------

    /// <summary>
    /// Pone el viaje en pausa. Reglas: solo un viaje en curso, sin otra pausa abierta, y la pausa no
    /// puede empezar antes del inicio del viaje ni antes de que terminara la pausa anterior.
    /// </summary>
    /// <param name="ultimaPausa">La pausa más reciente del viaje (abierta o cerrada), o null si no tiene.</param>
    public ViajePausa Pausar(ViajePausa? ultimaPausa, DateTime fecha, string? motivo, decimal? lat, decimal? lng)
    {
        ValidarEnCurso("pausar");
        ValidarPausaPropia(ultimaPausa);

        if (ultimaPausa is { EstaAbierta: true })
            throw new DomainException("El viaje ya está en pausa.");

        if (fecha < FechaInicio)
            throw new DomainException("La pausa no puede empezar antes del inicio del viaje.");

        if (ultimaPausa?.FechaFin is { } finAnterior && fecha < finAnterior)
            throw new DomainException("La pausa no puede empezar antes de que terminara la pausa anterior.");

        ValidarUbicacion(lat, lng, "de la pausa");

        return ViajePausa.Crear(EmpresaId, ViajeId, fecha, null, motivo, lat, lng);
    }

    /// <summary>Reanuda el viaje: cierra la pausa abierta.</summary>
    public void Reanudar(ViajePausa? pausaAbierta, DateTime fecha)
    {
        ValidarEnCurso("reanudar");

        if (pausaAbierta is not { EstaAbierta: true })
            throw new DomainException("El viaje no está en pausa.");

        ValidarPausaPropia(pausaAbierta);
        pausaAbierta.Cerrar(fecha);
    }

    /// <summary>
    /// Pausas de un viaje hecho sin conexión (llegan junto con el viaje). Se validan todas juntas:
    /// dentro del periodo del viaje, sin encimarse, y solo la última puede quedar abierta si el viaje
    /// sigue en curso. Si el viaje ya cerró, una pausa sin fin se cierra a la hora de fin del viaje.
    /// </summary>
    public IReadOnlyList<ViajePausa> RegistrarPausasSincronizadas(IEnumerable<DatosPausa> pausas)
    {
        var ordenadas = pausas.OrderBy(p => p.FechaInicio).ToList();
        var resultado = new List<ViajePausa>(ordenadas.Count);
        DateTime? finAnterior = null;

        for (var i = 0; i < ordenadas.Count; i++)
        {
            var datos = ordenadas[i];
            var esUltima = i == ordenadas.Count - 1;
            var fin = datos.FechaFin ?? (EstaEnCurso ? null : FechaFin);

            if (datos.FechaInicio < FechaInicio)
                throw new DomainException("Una pausa no puede empezar antes del inicio del viaje.");

            if (FechaFin is not null && (datos.FechaInicio > FechaFin || fin > FechaFin))
                throw new DomainException("Una pausa no puede quedar fuera del periodo del viaje.");

            if (finAnterior is not null && datos.FechaInicio < finAnterior)
                throw new DomainException("Las pausas no se pueden encimar.");

            if (fin is null && !esUltima)
                throw new DomainException("Solo la última pausa puede quedar abierta.");

            ValidarUbicacion(datos.UbicacionLat, datos.UbicacionLng, "de la pausa");

            resultado.Add(ViajePausa.Crear(EmpresaId, ViajeId, datos.FechaInicio, fin, datos.Motivo, datos.UbicacionLat, datos.UbicacionLng));
            finAnterior = fin;
        }

        return resultado;
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

    /// <summary>Se valida ANTES de tocar el viaje, para no dejarlo a medias si la pausa no cuadra.</summary>
    private void ValidarPausaAlTerminar(ViajePausa? pausaAbierta, DateTime fechaFin)
    {
        if (pausaAbierta is not { EstaAbierta: true })
            return;

        ValidarPausaPropia(pausaAbierta);

        if (fechaFin < pausaAbierta.FechaInicio)
            throw new DomainException("La fecha de fin no puede ser anterior al inicio de la pausa en curso.");
    }

    private static void CerrarPausaAlTerminar(ViajePausa? pausaAbierta, DateTime fechaFin)
    {
        if (pausaAbierta is { EstaAbierta: true })
            pausaAbierta.Cerrar(fechaFin);
    }

    private void ValidarPausaPropia(ViajePausa? pausa)
    {
        if (pausa is not null && pausa.ViajeId != ViajeId)
            throw new DomainException("La pausa no pertenece a este viaje.");
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
