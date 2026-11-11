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

        var usuario = await unitOfWork.Usuarios.GetByIdAsync(usuarioId, cancellationToken)
            ?? throw new NotFoundException($"No existe un usuario con el id {usuarioId}.");

        // La cuenta queda en la empresa de su empleado; una cuenta de otra empresa no se puede usar
        if (usuario.EmpresaId is not null && usuario.EmpresaId != empresaId)
            throw new ConflictException($"El usuario {usuarioId} pertenece a otra empresa.");

        usuario.AsignarEmpresa(empresaId);

        if (await unitOfWork.Empleados.UsuarioAsignadoAsync(usuarioId, excludeEmpleadoId, cancellationToken))
            throw new ConflictException($"El usuario {usuarioId} ya está asignado a otro empleado (activo o eliminado).");
    }
}
