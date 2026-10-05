using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Domain.Interfaces;
using MediatR;

namespace APIGasoline.Application.Features.RolPrivilegios.Commands;

public record DeleteRolPrivilegioCommand(int RolPrivilegioId) : IRequest;

public class DeleteRolPrivilegioCommandHandler : IRequestHandler<DeleteRolPrivilegioCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteRolPrivilegioCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(DeleteRolPrivilegioCommand request, CancellationToken cancellationToken)
    {
        var rolPrivilegio = await _unitOfWork.RolPrivilegios.GetByIdAsync(request.RolPrivilegioId, cancellationToken)
            ?? throw new NotFoundException($"No existe una asignación rol-privilegio con el id {request.RolPrivilegioId}.");

        _unitOfWork.RolPrivilegios.Remove(rolPrivilegio);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
