using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Application.Common.Interfaces;
using APIGasoline.Application.Common.Sesiones;
using APIGasoline.Domain.Enums;
using APIGasoline.Domain.Interfaces;
using MediatR;

namespace APIGasoline.Application.Features.Empleados.Commands;

/// <summary>
/// Soft delete: marca el empleado como eliminado (delete = true), no borra el registro.
/// Cierra las sesiones abiertas de su usuario.
/// </summary>
public record DeleteEmpleadoCommand(Guid EmpleadoId) : IRequest;

public class DeleteEmpleadoCommandHandler : IRequestHandler<DeleteEmpleadoCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUsuarioActual _usuarioActual;
    private readonly IDateTimeProvider _dateTime;

    public DeleteEmpleadoCommandHandler(IUnitOfWork unitOfWork, IUsuarioActual usuarioActual, IDateTimeProvider dateTime)
    {
        _unitOfWork = unitOfWork;
        _usuarioActual = usuarioActual;
        _dateTime = dateTime;
    }

    public async Task Handle(DeleteEmpleadoCommand request, CancellationToken cancellationToken)
    {
        var empleado = await _unitOfWork.Empleados.GetByIdAsync(request.EmpleadoId, cancellationToken)
            ?? throw new NotFoundException($"No existe un empleado con el id {request.EmpleadoId}.");

        empleado.Eliminar();

        await _unitOfWork.RevocarDeUsuarioAsync(
            empleado.UsuarioId, MotivoRevocacionSesion.EmpleadoDadoDeBaja, _dateTime.Now, _usuarioActual.UsuarioId,
            cancellationToken: cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
