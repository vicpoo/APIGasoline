using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Domain.Interfaces;
using MediatR;

namespace APIGasoline.Application.Features.Empleados.Commands;

/// <summary>
/// Soft delete: marca el empleado como eliminado (delete = true), no borra el registro.
/// </summary>
public record DeleteEmpleadoCommand(Guid EmpleadoId) : IRequest;

public class DeleteEmpleadoCommandHandler : IRequestHandler<DeleteEmpleadoCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteEmpleadoCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(DeleteEmpleadoCommand request, CancellationToken cancellationToken)
    {
        var empleado = await _unitOfWork.Empleados.GetByIdAsync(request.EmpleadoId, cancellationToken)
            ?? throw new NotFoundException($"No existe un empleado con el id {request.EmpleadoId}.");

        empleado.Eliminar();
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
