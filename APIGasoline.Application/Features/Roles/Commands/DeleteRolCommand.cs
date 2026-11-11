using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Domain.Interfaces;
using APIGasoline.Application.Common.Messaging;

namespace APIGasoline.Application.Features.Roles.Commands;

public sealed record DeleteRolCommand(Guid RolId) : ICommand;

public sealed class DeleteRolCommandHandler : ICommandHandler<DeleteRolCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteRolCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(DeleteRolCommand request, CancellationToken cancellationToken)
    {
        var rol = await _unitOfWork.Roles.GetByIdAsync(request.RolId, cancellationToken)
            ?? throw new NotFoundException($"No existe un rol con el id {request.RolId}.");

        _unitOfWork.Roles.Remove(rol);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
