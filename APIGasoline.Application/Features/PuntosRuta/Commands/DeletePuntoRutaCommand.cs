using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Domain.Interfaces;
using MediatR;

namespace APIGasoline.Application.Features.PuntosRuta.Commands;

public record DeletePuntoRutaCommand(int PuntoRutaId) : IRequest;

public class DeletePuntoRutaCommandHandler : IRequestHandler<DeletePuntoRutaCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeletePuntoRutaCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(DeletePuntoRutaCommand request, CancellationToken cancellationToken)
    {
        var puntoRuta = await _unitOfWork.PuntosRuta.GetByIdAsync(request.PuntoRutaId, cancellationToken)
            ?? throw new NotFoundException($"No existe un punto de ruta con el id {request.PuntoRutaId}.");

        _unitOfWork.PuntosRuta.Remove(puntoRuta);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
