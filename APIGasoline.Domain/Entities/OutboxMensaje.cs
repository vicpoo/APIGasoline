using APIGasoline.Domain.Enums;
using APIGasoline.Domain.Exceptions;
using APIGasoline.Domain.Outbox;

namespace APIGasoline.Domain.Entities;

public class OutboxMensaje
{
    public const int TipoMaxLength = 100;
    public const int UltimoErrorMaxLength = 500;

    public Guid OutboxMensajeId { get; private set; } = Guid.CreateVersion7();
    public Guid EmpresaId { get; private set; }
    public string Tipo { get; private set; } = string.Empty;
    public Guid AgregadoId { get; private set; }
    public string Payload { get; private set; } = string.Empty;
    public EstadoOutbox Estado { get; private set; } = EstadoOutbox.Pendiente;
    public int Intentos { get; private set; }
    public DateTime ProximoIntento { get; private set; }
    public string? UltimoError { get; private set; }
    public DateTime FechaCreacion { get; private set; }
    public DateTime? FechaProcesado { get; private set; }

    // Requerido por Entity Framework
    private OutboxMensaje()
    {
    }

    public static OutboxMensaje Crear(Guid empresaId, string tipo, Guid agregadoId, string payload, DateTime ahora)
    {
        if (empresaId == Guid.Empty)
            throw new DomainException("El id de la empresa es obligatorio.");

        if (agregadoId == Guid.Empty)
            throw new DomainException("El id del registro que origina el mensaje es obligatorio.");

        var tipoNormalizado = tipo?.Trim() ?? string.Empty;

        if (tipoNormalizado.Length == 0)
            throw new DomainException("El tipo del mensaje es obligatorio.");

        if (tipoNormalizado.Length > TipoMaxLength)
            throw new DomainException($"El tipo del mensaje no puede exceder {TipoMaxLength} caracteres.");

        if (string.IsNullOrWhiteSpace(payload))
            throw new DomainException("El contenido del mensaje es obligatorio.");

        if (ahora == default)
            throw new DomainException("La fecha de creación es obligatoria.");

        return new OutboxMensaje
        {
            EmpresaId = empresaId,
            Tipo = tipoNormalizado,
            AgregadoId = agregadoId,
            Payload = payload,
            Estado = EstadoOutbox.Pendiente,
            ProximoIntento = ahora,
            FechaCreacion = ahora
        };
    }

    public bool EstaListo(DateTime ahora)
        => (Estado is EstadoOutbox.Pendiente or EstadoOutbox.Procesando) && ProximoIntento <= ahora;

    /// <summary>
    /// Lo aparta para un worker. Si el worker se cae a la mitad, al vencer el bloqueo
    /// otro lo vuelve a tomar (ProximoIntento = ahora + bloqueo).
    /// </summary>
    public void MarcarProcesando(DateTime ahora, TimeSpan bloqueo)
    {
        if (!EstaListo(ahora))
            throw new DomainException("El mensaje no está listo para procesarse.");

        if (bloqueo <= TimeSpan.Zero)
            throw new DomainException("El tiempo de bloqueo debe ser mayor a cero.");

        Estado = EstadoOutbox.Procesando;
        ProximoIntento = ahora + bloqueo;
    }

    /// <param name="nota">Aclaración opcional, por ejemplo "Sin dispositivos activos".</param>
    public void MarcarEnviado(DateTime ahora, string? nota = null)
    {
        ValidarNoTerminado();

        Estado = EstadoOutbox.Enviado;
        Intentos++;
        UltimoError = Recortar(nota);
        FechaProcesado = ahora;
    }

    public void RegistrarFallo(string error, DateTime ahora)
    {
        ValidarNoTerminado();

        Intentos++;
        UltimoError = Recortar(error) ?? "Error desconocido.";

        if (PoliticaReintentos.SeAgotaron(Intentos))
        {
            Estado = EstadoOutbox.Fallido;
            FechaProcesado = ahora;
            return;
        }

        Estado = EstadoOutbox.Pendiente;
        ProximoIntento = ahora + PoliticaReintentos.EsperaTras(Intentos);
    }

    /// <summary>Un mensaje fallido se puede volver a intentar a mano (por ejemplo, después de corregir Firebase).</summary>
    public void Reintentar(DateTime ahora)
    {
        if (Estado != EstadoOutbox.Fallido)
            throw new DomainException("Solo se puede reintentar un mensaje fallido.");

        Estado = EstadoOutbox.Pendiente;
        Intentos = 0;
        ProximoIntento = ahora;
        FechaProcesado = null;
    }

    private void ValidarNoTerminado()
    {
        if (Estado is EstadoOutbox.Enviado or EstadoOutbox.Fallido)
            throw new DomainException("El mensaje ya fue procesado.");
    }

    private static string? Recortar(string? texto)
    {
        var limpio = string.IsNullOrWhiteSpace(texto) ? null : texto.Trim();

        return limpio is { Length: > UltimoErrorMaxLength }
            ? limpio[..(UltimoErrorMaxLength - 3)] + "..."
            : limpio;
    }
}
