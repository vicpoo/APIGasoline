using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Domain.Interfaces;

namespace APIGasoline.Application.Features.CentrosCosto.Common;

/// <summary>
/// Regla compartida por viajes y cargas: el centro de costos (si se manda) debe existir,
/// pertenecer a la misma empresa y estar activo.
/// </summary>
internal static class CentroCostoValidator
{
    public static async Task ValidarAsync(IUnitOfWork unitOfWork, Guid empresaId, Guid? centroCostoId, CancellationToken cancellationToken)
    {
        if (centroCostoId is null)
            return;

        var centro = await unitOfWork.CentrosCosto.GetByIdAsync(centroCostoId.Value, cancellationToken)
            ?? throw new NotFoundException($"No existe un centro de costo con el id {centroCostoId}.");

        if (centro.EmpresaId != empresaId)
            throw new BadRequestException($"El centro de costo {centroCostoId} no pertenece a la empresa {empresaId}.");

        if (!centro.Activo)
            throw new BadRequestException($"El centro de costo {centro.Nombre} está inactivo.");
    }
}
