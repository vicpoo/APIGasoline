using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Domain.Interfaces;
using APIGasoline.Application.Common.Messaging;

namespace APIGasoline.Application.Features.Corporativos.Commands;

public sealed record DeleteCorporativoCommand(Guid CorporativoId) : ICommand;

public sealed class DeleteCorporativoCommandHandler : ICommandHandler<DeleteCorporativoCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteCorporativoCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(DeleteCorporativoCommand request, CancellationToken cancellationToken)
    {
        var corporativo = await _unitOfWork.Corporativos.GetByIdAsync(request.CorporativoId, cancellationToken)
            ?? throw new NotFoundException($"No existe un corporativo con el id {request.CorporativoId}.");

        if (await _unitOfWork.Divisiones.ExistsByCorporativoAsync(corporativo.CorporativoId, cancellationToken))
            throw new ConflictException("No se puede eliminar el corporativo porque tiene divisiones registradas.");

        if (await _unitOfWork.Empresas.ExistsByCorporativoAsync(corporativo.CorporativoId, cancellationToken))
            throw new ConflictException("No se puede eliminar el corporativo porque tiene empresas registradas.");

        _unitOfWork.Corporativos.Remove(corporativo);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
