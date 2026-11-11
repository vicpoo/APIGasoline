using APIGasoline.Application.Common.Interfaces;
using APIGasoline.Application.Common.Security;
using APIGasoline.Domain.Interfaces;
using APIGasoline.Application.Common.Messaging;

namespace APIGasoline.Application.Features.Notificaciones.Queries;

/// <summary>
/// Número para el "badge" de la campana en la app.
/// </summary>
public sealed record ContarNotificacionesNoLeidasQuery(Guid EmpleadoDestinatarioId) : IQuery<int>, IRecursoPropio
{
    /// <summary>Su bandeja, o la de otros con notificacion.enviar.</summary>
    public bool EsPropio(IUsuarioActual usuario)
        => EmpleadoDestinatarioId == usuario.EmpleadoId;

    public string PrivilegioParaOtros => Permisos.NotificacionEnviar;
}

public sealed class ContarNotificacionesNoLeidasQueryHandler : IQueryHandler<ContarNotificacionesNoLeidasQuery, int>
{
    private readonly IUnitOfWork _unitOfWork;

    public ContarNotificacionesNoLeidasQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public Task<int> Handle(ContarNotificacionesNoLeidasQuery request, CancellationToken cancellationToken)
        => _unitOfWork.Notificaciones.ContarNoLeidasAsync(request.EmpleadoDestinatarioId, cancellationToken);
}
