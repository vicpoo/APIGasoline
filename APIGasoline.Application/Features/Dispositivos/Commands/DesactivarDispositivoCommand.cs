using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Domain.Interfaces;
using APIGasoline.Application.Common.Messaging;

namespace APIGasoline.Application.Features.Dispositivos.Commands;

/// <summary>Al cerrar sesión: ese celular deja de recibir push. Se conserva el registro.</summary>
public sealed record DesactivarDispositivoCommand(Guid DispositivoEmpleadoId) : ICommand;

public sealed class DesactivarDispositivoCommandHandler : ICommandHandler<DesactivarDispositivoCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public DesactivarDispositivoCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(DesactivarDispositivoCommand request, CancellationToken cancellationToken)
    {
        var dispositivo = await _unitOfWork.Dispositivos.GetByIdAsync(request.DispositivoEmpleadoId, cancellationToken)
            ?? throw new NotFoundException($"No existe un dispositivo con el id {request.DispositivoEmpleadoId}.");

        dispositivo.Desactivar();
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
