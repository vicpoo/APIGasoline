using APIGasoline.Domain.Interfaces;
using MediatR;

namespace APIGasoline.Application.Features.Notificaciones.Queries;

/// <summary>
/// Número para el "badge" de la campana en la app.
/// </summary>
public record ContarNotificacionesNoLeidasQuery(Guid EmpleadoDestinatarioId) : IRequest<int>;

public class ContarNotificacionesNoLeidasQueryHandler : IRequestHandler<ContarNotificacionesNoLeidasQuery, int>
{
    private readonly IUnitOfWork _unitOfWork;

    public ContarNotificacionesNoLeidasQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public Task<int> Handle(ContarNotificacionesNoLeidasQuery request, CancellationToken cancellationToken)
        => _unitOfWork.Notificaciones.ContarNoLeidasAsync(request.EmpleadoDestinatarioId, cancellationToken);
}
