using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Domain.Interfaces;

namespace APIGasoline.Application.Features.Empresas.Common;

/// <summary>
/// Valida que el corporativo y la división existan y que la división pertenezca al corporativo.
/// Se comparte entre Create y Update.
/// </summary>
internal static class EmpresaOrganizacionValidator
{
    public static async Task ValidarAsync(IUnitOfWork unitOfWork, int? corporativoId, int? divisionId, CancellationToken cancellationToken)
    {
        if (corporativoId is not null
            && !await unitOfWork.Corporativos.ExistsAsync(corporativoId.Value, cancellationToken))
            throw new NotFoundException($"No existe un corporativo con el id {corporativoId}.");

        if (divisionId is null)
            return;

        var division = await unitOfWork.Divisiones.GetByIdAsync(divisionId.Value, cancellationToken)
            ?? throw new NotFoundException($"No existe una división con el id {divisionId}.");

        if (division.CorporativoId != corporativoId)
            throw new BadRequestException($"La división {divisionId} no pertenece al corporativo {corporativoId}.");
    }
}
