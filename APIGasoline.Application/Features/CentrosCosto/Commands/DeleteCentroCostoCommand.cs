using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Domain.Interfaces;
using MediatR;

namespace APIGasoline.Application.Features.CentrosCosto.Commands;

public record DeleteCentroCostoCommand(Guid CentroCostoId) : IRequest;

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

        // v6: viajes y cargas lo referencian (FK restrict). Con historial se desactiva, no se borra.
        if (await _unitOfWork.Viajes.ExistsByCentroCostoAsync(centroCosto.CentroCostoId, cancellationToken)
            || await _unitOfWork.CargasCombustible.ExistsByCentroCostoAsync(centroCosto.CentroCostoId, cancellationToken))
            throw new ConflictException("El centro de costo ya tiene viajes o cargas registrados. Desactívalo en lugar de eliminarlo.");

        _unitOfWork.CentrosCosto.Remove(centroCosto);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
