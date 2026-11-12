using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Domain.Entities;
using APIGasoline.Domain.Interfaces;

namespace APIGasoline.Application.Features.CentrosCosto.Common;

/// <summary>
/// El corporativo y la división de un centro de costo (opcionales) deben ser de la empresa del centro,
/// y si vienen los dos, estar ligados. La BD lo vuelve a exigir con FK compuestas.
/// Se comparte entre Create y Update.
/// </summary>
internal static class CentroCostoOrganizacionValidator
{
    public static async Task ValidarAsync(IUnitOfWork unitOfWork, CentroCosto centroCosto, CancellationToken cancellationToken)
    {
        if (centroCosto.CorporativoId is { } corporativoId)
        {
            var corporativo = await unitOfWork.Corporativos.GetByIdAsync(corporativoId, cancellationToken)
                ?? throw new NotFoundException($"No existe un corporativo con el id {corporativoId}.");

            if (corporativo.EmpresaId != centroCosto.EmpresaId)
                throw new BadRequestException($"El corporativo {corporativoId} no es de la empresa {centroCosto.EmpresaId}.");
        }

        if (centroCosto.DivisionId is { } divisionId)
        {
            var division = await unitOfWork.Divisiones.GetByIdAsync(divisionId, cancellationToken)
                ?? throw new NotFoundException($"No existe una división con el id {divisionId}.");

            if (division.EmpresaId != centroCosto.EmpresaId)
                throw new BadRequestException($"La división {divisionId} no es de la empresa {centroCosto.EmpresaId}.");
        }

        if (centroCosto.CorporativoId is { } c && centroCosto.DivisionId is { } d
            && !await unitOfWork.CorporativoDivisiones.ExistsAsync(c, d, cancellationToken))
            throw new BadRequestException($"La división {d} no está ligada al corporativo {c}.");
    }
}
