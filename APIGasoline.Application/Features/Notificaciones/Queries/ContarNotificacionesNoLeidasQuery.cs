using APIGasoline.Application.Common.Interfaces;
using APIGasoline.Application.Common.Security;
using APIGasoline.Domain.Interfaces;
using MediatR;

namespace APIGasoline.Application.Features.Notificaciones.Queries;

/// <summary>
/// Número para el "badge" de la campana en la app.
/// </summary>
public record ContarNotificacionesNoLeidasQuery(Guid EmpleadoDestinatarioId) : IRequest<int>, IRecursoPropio
{
    /// <summary>Su bandeja, o la de otros con notificacion.enviar.</summary>
    public bool EsPropio(IUsuarioActual usuario)
        => EmpleadoDestinatarioId == usuario.EmpleadoId;

    public string PrivilegioParaOtros => Permisos.NotificacionEnviar;
}

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
