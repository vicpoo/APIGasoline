using APIGasoline.Application.Common.Interfaces;
using APIGasoline.Domain.Entities;
using APIGasoline.Domain.Interfaces;

namespace APIGasoline.Application.Common.Outbox;

/// <summary>
/// Agrega la notificación a la bandeja y, en el mismo SaveChanges, su mensaje en el outbox
/// para que el worker mande el push. Así nunca queda una sin la otra.
/// </summary>
public class NotificadorPush
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _dateTime;

    public NotificadorPush(IUnitOfWork unitOfWork, IDateTimeProvider dateTime)
    {
        _unitOfWork = unitOfWork;
        _dateTime = dateTime;
    }

    public async Task AgregarAsync(Notificacion notificacion, CancellationToken cancellationToken = default)
    {
        await _unitOfWork.Notificaciones.AddAsync(notificacion, cancellationToken);
        await _unitOfWork.OutboxMensajes.AddAsync(MensajesPush.CrearMensaje(notificacion, _dateTime.Now), cancellationToken);
    }

    public async Task AgregarAsync(IEnumerable<Notificacion> notificaciones, CancellationToken cancellationToken = default)
    {
        foreach (var notificacion in notificaciones)
            await AgregarAsync(notificacion, cancellationToken);
    }
}
