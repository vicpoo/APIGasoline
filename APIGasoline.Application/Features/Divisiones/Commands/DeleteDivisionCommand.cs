using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Application.Common.Messaging;
using APIGasoline.Domain.Interfaces;

namespace APIGasoline.Application.Features.Divisiones.Commands;

public sealed record DeleteDivisionCommand(Guid DivisionId) : ICommand;

public sealed class DeleteDivisionCommandHandler : ICommandHandler<DeleteDivisionCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteDivisionCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(DeleteDivisionCommand request, CancellationToken cancellationToken)
    {
        var division = await _unitOfWork.Divisiones.GetByIdAsync(request.DivisionId, cancellationToken)
            ?? throw new NotFoundException($"No existe una división con el id {request.DivisionId}.");

        if (await _unitOfWork.CorporativoDivisiones.ExistsByDivisionAsync(division.DivisionId, cancellationToken))
            throw new ConflictException("No se puede eliminar la división porque está ligada a corporativos. Desligala primero.");

        if (await _unitOfWork.CentrosCosto.ExistsByDivisionAsync(division.DivisionId, cancellationToken))
            throw new ConflictException("No se puede eliminar la división porque tiene centros de costo. Desactívala en lugar de eliminarla.");

        _unitOfWork.Divisiones.Remove(division);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
