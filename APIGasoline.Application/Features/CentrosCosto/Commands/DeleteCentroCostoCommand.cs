using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Domain.Interfaces;
using MediatR;

namespace APIGasoline.Application.Features.CentrosCosto.Commands;

public record DeleteCentroCostoCommand(int CentroCostoId) : IRequest;

public class DeleteCentroCostoCommandHandler : IRequestHandler<DeleteCentroCostoCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteCentroCostoCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(DeleteCentroCostoCommand request, CancellationToken cancellationToken)
    {
        var centroCosto = await _unitOfWork.CentrosCosto.GetByIdAsync(request.CentroCostoId, cancellationToken)
            ?? throw new NotFoundException($"No existe un centro de costo con el id {request.CentroCostoId}.");

        _unitOfWork.CentrosCosto.Remove(centroCosto);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
