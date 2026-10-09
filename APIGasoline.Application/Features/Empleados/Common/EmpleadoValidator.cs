using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Domain.Interfaces;

namespace APIGasoline.Application.Features.Empleados.Common;

/// <summary>
/// Validaciones contra otras entidades, compartidas entre Create y Update.
/// </summary>
internal static class EmpleadoValidator
{
    public static async Task ValidarAsync(IUnitOfWork unitOfWork, Guid empresaId, Guid usuarioId, Guid? excludeEmpleadoId, CancellationToken cancellationToken)
    {
        // Las empresas eliminadas (soft delete) no cuentan como existentes
        if (!await unitOfWork.Empresas.ExistsAsync(empresaId, cancellationToken))
            throw new NotFoundException($"No existe una empresa con el id {empresaId}.");

        if (!await unitOfWork.Usuarios.ExistsAsync(usuarioId, cancellationToken))
            throw new NotFoundException($"No existe un usuario con el id {usuarioId}.");

        if (await unitOfWork.Empleados.UsuarioAsignadoAsync(usuarioId, excludeEmpleadoId, cancellationToken))
            throw new ConflictException($"El usuario {usuarioId} ya está asignado a otro empleado (activo o eliminado).");
    }
}
