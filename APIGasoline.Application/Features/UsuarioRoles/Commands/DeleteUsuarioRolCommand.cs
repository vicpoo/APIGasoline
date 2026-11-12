using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Application.Common.Security;
using APIGasoline.Application.Common.Interfaces;
using APIGasoline.Domain.Interfaces;
using APIGasoline.Application.Common.Messaging;

namespace APIGasoline.Application.Features.UsuarioRoles.Commands;

public sealed record DeleteUsuarioRolCommand(Guid UsuarioRolId) : ICommand;

public sealed class DeleteUsuarioRolCommandHandler : ICommandHandler<DeleteUsuarioRolCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    private readonly IUsuarioActual _usuarioActual;

    public DeleteUsuarioRolCommandHandler(IUnitOfWork unitOfWork, IUsuarioActual usuarioActual)
    {
        _unitOfWork = unitOfWork;
        _usuarioActual = usuarioActual;
    }

    public async Task Handle(DeleteUsuarioRolCommand request, CancellationToken cancellationToken)
    {
        var usuarioRol = await _unitOfWork.UsuarioRoles.GetByIdAsync(request.UsuarioRolId, cancellationToken)
            ?? throw new NotFoundException($"No existe una asignación usuario-rol con el id {request.UsuarioRolId}.");

        // Techo de privilegios: no quitas un rol que tú no podrías dar
        await TechoPrivilegios.ExigirRolAsync(_unitOfWork, _usuarioActual, usuarioRol.RolId, cancellationToken);

        _unitOfWork.UsuarioRoles.Remove(usuarioRol);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
