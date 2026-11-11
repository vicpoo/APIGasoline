using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Domain.Interfaces;
using APIGasoline.Application.Common.Messaging;

namespace APIGasoline.Application.Features.UsuarioRoles.Commands;

public sealed record DeleteUsuarioRolCommand(Guid UsuarioRolId) : ICommand;

public sealed class DeleteUsuarioRolCommandHandler : ICommandHandler<DeleteUsuarioRolCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteUsuarioRolCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(DeleteUsuarioRolCommand request, CancellationToken cancellationToken)
    {
        var usuarioRol = await _unitOfWork.UsuarioRoles.GetByIdAsync(request.UsuarioRolId, cancellationToken)
            ?? throw new NotFoundException($"No existe una asignación usuario-rol con el id {request.UsuarioRolId}.");

        _unitOfWork.UsuarioRoles.Remove(usuarioRol);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
