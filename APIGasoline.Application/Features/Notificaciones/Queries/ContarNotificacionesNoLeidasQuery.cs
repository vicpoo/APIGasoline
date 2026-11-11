using APIGasoline.Application.Common.Interfaces;
using APIGasoline.Application.Common.Exceptions;
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

    public async Task<int> Handle(ContarNotificacionesNoLeidasQuery request, CancellationToken cancellationToken)
    {
        // Un empleado de otra empresa "no existe" (filtro por empresa): 404 en vez de contar 0
        if (!await _unitOfWork.Empleados.ExistsAsync(request.EmpleadoDestinatarioId, cancellationToken))
            throw new NotFoundException($"No existe un empleado con el id {request.EmpleadoDestinatarioId}.");

        return await _unitOfWork.Notificaciones.ContarNoLeidasAsync(request.EmpleadoDestinatarioId, cancellationToken);
    }
}
