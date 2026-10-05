using APIGasoline.Domain.Interfaces;
using MediatR;

namespace APIGasoline.Application.Features.Notificaciones.Commands;

/// <summary>
/// Marca como leídas todas las notificaciones pendientes de un empleado. Devuelve cuántas se marcaron.
/// </summary>
public record MarcarTodasNotificacionesLeidasCommand(int EmpleadoDestinatarioId) : IRequest<int>;

public class MarcarTodasNotificacionesLeidasCommandHandler : IRequestHandler<MarcarTodasNotificacionesLeidasCommand, int>
{
    private readonly IUnitOfWork _unitOfWork;

    public MarcarTodasNotificacionesLeidasCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<int> Handle(MarcarTodasNotificacionesLeidasCommand request, CancellationToken cancellationToken)
    {
        var noLeidas = await _unitOfWork.Notificaciones.GetNoLeidasAsync(request.EmpleadoDestinatarioId, cancellationToken);

        foreach (var notificacion in noLeidas)
            notificacion.MarcarLeida();

        if (noLeidas.Count > 0)
            await _unitOfWork.SaveChangesAsync(cancellationToken);

        return noLeidas.Count;
    }
}
