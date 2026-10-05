using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Domain.Interfaces;
using MediatR;

namespace APIGasoline.Application.Features.CargasCombustible.Commands;

public record DeleteCargaCombustibleCommand(int CargaCombustibleId) : IRequest;

public class DeleteCargaCombustibleCommandHandler : IRequestHandler<DeleteCargaCombustibleCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteCargaCombustibleCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(DeleteCargaCombustibleCommand request, CancellationToken cancellationToken)
    {
        var carga = await _unitOfWork.CargasCombustible.GetByIdAsync(request.CargaCombustibleId, cancellationToken)
            ?? throw new NotFoundException($"No existe una carga de combustible con el id {request.CargaCombustibleId}.");

        _unitOfWork.CargasCombustible.Remove(carga);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
