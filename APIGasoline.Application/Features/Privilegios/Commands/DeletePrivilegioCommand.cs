using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Domain.Interfaces;
using APIGasoline.Application.Common.Messaging;

namespace APIGasoline.Application.Features.Privilegios.Commands;

public sealed record DeletePrivilegioCommand(Guid PrivilegioId) : ICommand;

public sealed class DeletePrivilegioCommandHandler : ICommandHandler<DeletePrivilegioCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeletePrivilegioCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(DeletePrivilegioCommand request, CancellationToken cancellationToken)
    {
        var privilegio = await _unitOfWork.Privilegios.GetByIdAsync(request.PrivilegioId, cancellationToken)
            ?? throw new NotFoundException($"No existe un privilegio con el id {request.PrivilegioId}.");

        _unitOfWork.Privilegios.Remove(privilegio);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
