using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Domain.Interfaces;

namespace APIGasoline.Application.Features.SolicitudesReserva.Commands;

internal static class SolicitudReservaValidator
{
    /// <summary>
    /// El confirmador debe existir (no eliminado) y pertenecer a la misma empresa de la solicitud.
    /// </summary>
    public static async Task ValidarConfirmadorAsync(IUnitOfWork unitOfWork, int empresaId, int empleadoConfirmadorId, CancellationToken cancellationToken)
    {
        var confirmador = await unitOfWork.Empleados.GetByIdAsync(empleadoConfirmadorId, cancellationToken)
            ?? throw new NotFoundException($"No existe un empleado con el id {empleadoConfirmadorId}.");

        if (confirmador.EmpresaId != empresaId)
            throw new BadRequestException($"El empleado {empleadoConfirmadorId} no pertenece a la empresa {empresaId}.");
    }
}
