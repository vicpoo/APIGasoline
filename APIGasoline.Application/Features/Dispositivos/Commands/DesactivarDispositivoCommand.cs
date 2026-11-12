using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Application.Common.Interfaces;
using APIGasoline.Application.Common.Security;
using APIGasoline.Domain.Interfaces;
using APIGasoline.Application.Common.Messaging;

namespace APIGasoline.Application.Features.Dispositivos.Commands;

/// <summary>Al cerrar sesión: ese celular deja de recibir push. Se conserva el registro.</summary>
public sealed record DesactivarDispositivoCommand(Guid DispositivoEmpleadoId) : ICommand;

public sealed class DesactivarDispositivoCommandHandler : ICommandHandler<DesactivarDispositivoCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUsuarioActual _usuarioActual;

    public DesactivarDispositivoCommandHandler(IUnitOfWork unitOfWork, IUsuarioActual usuarioActual)
    {
        _unitOfWork = unitOfWork;
        _usuarioActual = usuarioActual;
    }

    public async Task Handle(DesactivarDispositivoCommand request, CancellationToken cancellationToken)
    {
        var dispositivo = await _unitOfWork.Dispositivos.GetByIdAsync(request.DispositivoEmpleadoId, cancellationToken)
            ?? throw new NotFoundException($"No existe un dispositivo con el id {request.DispositivoEmpleadoId}.");

        // Cada quien desactiva su celular; el de otro empleado, solo quien administra empleados
        await _usuarioActual.ValidarPropioOAlgunPrivilegioAsync(
            dispositivo.EmpleadoId == _usuarioActual.EmpleadoId,
            [Permisos.EmpleadoGestionar],
            "Solo puedes desactivar tus propios dispositivos",
            cancellationToken);

        dispositivo.Desactivar();
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
