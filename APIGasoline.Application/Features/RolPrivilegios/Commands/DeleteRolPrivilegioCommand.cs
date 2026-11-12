using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Application.Common.Security;
using APIGasoline.Application.Common.Interfaces;
using APIGasoline.Domain.Interfaces;
using APIGasoline.Application.Common.Messaging;

namespace APIGasoline.Application.Features.RolPrivilegios.Commands;

public sealed record DeleteRolPrivilegioCommand(Guid RolPrivilegioId) : ICommand;

public sealed class DeleteRolPrivilegioCommandHandler : ICommandHandler<DeleteRolPrivilegioCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    private readonly IUsuarioActual _usuarioActual;

    public DeleteRolPrivilegioCommandHandler(IUnitOfWork unitOfWork, IUsuarioActual usuarioActual)
    {
        _unitOfWork = unitOfWork;
        _usuarioActual = usuarioActual;
    }

    public async Task Handle(DeleteRolPrivilegioCommand request, CancellationToken cancellationToken)
    {
        var rolPrivilegio = await _unitOfWork.RolPrivilegios.GetByIdAsync(request.RolPrivilegioId, cancellationToken)
            ?? throw new NotFoundException($"No existe una asignación rol-privilegio con el id {request.RolPrivilegioId}.");

        // Techo de privilegios: solo quitas lo que tú también tienes
        var privilegio = await _unitOfWork.Privilegios.GetByIdAsync(rolPrivilegio.PrivilegioId, cancellationToken);
        if (privilegio is not null)
            await TechoPrivilegios.ExigirPrivilegioAsync(_usuarioActual, privilegio.Clave, cancellationToken);

        _unitOfWork.RolPrivilegios.Remove(rolPrivilegio);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
