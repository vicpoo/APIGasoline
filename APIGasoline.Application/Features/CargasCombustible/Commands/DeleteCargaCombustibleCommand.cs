using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Application.Common.Interfaces;
using APIGasoline.Domain.Interfaces;
using MediatR;

namespace APIGasoline.Application.Features.CargasCombustible.Commands;

public record DeleteCargaCombustibleCommand(int CargaCombustibleId) : IRequest;

public class DeleteCargaCombustibleCommandHandler : IRequestHandler<DeleteCargaCombustibleCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFileStorageService _storage;

    public DeleteCargaCombustibleCommandHandler(IUnitOfWork unitOfWork, IFileStorageService storage)
    {
        _unitOfWork = unitOfWork;
        _storage = storage;
    }

    public async Task Handle(DeleteCargaCombustibleCommand request, CancellationToken cancellationToken)
    {
        var carga = await _unitOfWork.CargasCombustible.GetByIdAsync(request.CargaCombustibleId, cancellationToken)
            ?? throw new NotFoundException($"No existe una carga de combustible con el id {request.CargaCombustibleId}.");

        var comprobante = carga.FotoComprobante;

        _unitOfWork.CargasCombustible.Remove(carga);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // El registro ya se borró: si falla la limpieza del archivo solo se registra en el log
        await _storage.IntentarEliminarAsync(comprobante);
    }
}
