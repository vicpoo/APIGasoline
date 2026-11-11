using APIGasoline.Application.Common.Interfaces;
using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Application.Common.Security;
using APIGasoline.Domain.Interfaces;
using APIGasoline.Application.Common.Messaging;

namespace APIGasoline.Application.Features.Notificaciones.Commands;

/// <summary>
/// Marca como leídas todas las notificaciones pendientes de un empleado. Devuelve cuántas se marcaron.
/// </summary>
public sealed record MarcarTodasNotificacionesLeidasCommand(Guid EmpleadoDestinatarioId) : ICommand<int>, IRecursoPropio
{
    /// <summary>Su bandeja, o la de otros con notificacion.enviar.</summary>
    public bool EsPropio(IUsuarioActual usuario)
        => EmpleadoDestinatarioId == usuario.EmpleadoId;

    public string PrivilegioParaOtros => Permisos.NotificacionEnviar;
}

public sealed class MarcarTodasNotificacionesLeidasCommandHandler : ICommandHandler<MarcarTodasNotificacionesLeidasCommand, int>
{
    private readonly IUnitOfWork _unitOfWork;

    public MarcarTodasNotificacionesLeidasCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<int> Handle(MarcarTodasNotificacionesLeidasCommand request, CancellationToken cancellationToken)
    {
        // Un empleado de otra empresa "no existe" (filtro por empresa): 404 en vez de marcar 0
        if (!await _unitOfWork.Empleados.ExistsAsync(request.EmpleadoDestinatarioId, cancellationToken))
            throw new NotFoundException($"No existe un empleado con el id {request.EmpleadoDestinatarioId}.");

        var noLeidas = await _unitOfWork.Notificaciones.GetNoLeidasAsync(request.EmpleadoDestinatarioId, cancellationToken);

        foreach (var notificacion in noLeidas)
            notificacion.MarcarLeida();

        if (noLeidas.Count > 0)
            await _unitOfWork.SaveChangesAsync(cancellationToken);

        return noLeidas.Count;
    }
}
