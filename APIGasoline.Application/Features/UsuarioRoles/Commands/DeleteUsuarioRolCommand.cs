using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Domain.Interfaces;
using MediatR;

namespace APIGasoline.Application.Features.UsuarioRoles.Commands;

public record DeleteUsuarioRolCommand(int UsuarioRolId) : IRequest;

public class DeleteUsuarioRolCommandHandler : IRequestHandler<DeleteUsuarioRolCommand>
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
