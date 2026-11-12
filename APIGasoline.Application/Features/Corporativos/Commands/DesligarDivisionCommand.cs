using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Application.Common.Messaging;
using APIGasoline.Domain.Interfaces;

namespace APIGasoline.Application.Features.Corporativos.Commands;

/// <summary>Quita la liga entre un corporativo y una división, si ningún centro de costo usa esa pareja.</summary>
public sealed record DesligarDivisionCommand(Guid CorporativoId, Guid DivisionId) : ICommand;

public sealed class DesligarDivisionCommandHandler : ICommandHandler<DesligarDivisionCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public DesligarDivisionCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(DesligarDivisionCommand request, CancellationToken cancellationToken)
    {
        var liga = await _unitOfWork.CorporativoDivisiones.GetAsync(request.CorporativoId, request.DivisionId, cancellationToken)
            ?? throw new NotFoundException($"La división {request.DivisionId} no está ligada al corporativo {request.CorporativoId}.");

        if (await _unitOfWork.CentrosCosto.ExistsByLigaAsync(liga.CorporativoId, liga.DivisionId, cancellationToken))
            throw new ConflictException("No se puede desligar: hay centros de costo de ese corporativo y esa división.");

        _unitOfWork.CorporativoDivisiones.Remove(liga);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
