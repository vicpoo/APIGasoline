using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Domain.Interfaces;
using MediatR;

namespace APIGasoline.Application.Features.Roles.Commands;

public record DeleteRolCommand(int RolId) : IRequest;

public class DeleteRolCommandHandler : IRequestHandler<DeleteRolCommand>
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
