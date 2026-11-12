using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Application.Common.Security;
using APIGasoline.Application.Common.Interfaces;
using APIGasoline.Domain.Interfaces;
using APIGasoline.Application.Common.Messaging;

namespace APIGasoline.Application.Features.Roles.Commands;

public sealed record DeleteRolCommand(Guid RolId) : ICommand;

public sealed class DeleteRolCommandHandler : ICommandHandler<DeleteRolCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    private readonly IUsuarioActual _usuarioActual;

    public DeleteRolCommandHandler(IUnitOfWork unitOfWork, IUsuarioActual usuarioActual)
    {
        _unitOfWork = unitOfWork;
        _usuarioActual = usuarioActual;
    }

    public async Task Handle(DeleteRolCommand request, CancellationToken cancellationToken)
    {
        var rol = await _unitOfWork.Roles.GetByIdAsync(request.RolId, cancellationToken)
            ?? throw new NotFoundException($"No existe un rol con el id {request.RolId}.");

        // Techo de privilegios: no borras un rol con privilegios que tú no tienes
        await TechoPrivilegios.ExigirRolAsync(_unitOfWork, _usuarioActual, rol.RolId, cancellationToken);

        _unitOfWork.Roles.Remove(rol);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
