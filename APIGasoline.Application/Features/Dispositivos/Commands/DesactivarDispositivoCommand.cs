using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Domain.Interfaces;
using MediatR;

namespace APIGasoline.Application.Features.Dispositivos.Commands;

/// <summary>Al cerrar sesión: ese celular deja de recibir push. Se conserva el registro.</summary>
public record DesactivarDispositivoCommand(Guid DispositivoEmpleadoId) : IRequest;

public class DesactivarDispositivoCommandHandler : IRequestHandler<DesactivarDispositivoCommand>
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
