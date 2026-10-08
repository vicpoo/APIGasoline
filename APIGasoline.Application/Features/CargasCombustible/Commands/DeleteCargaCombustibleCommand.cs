using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Application.Common.Interfaces;
using APIGasoline.Domain.Interfaces;
using MediatR;
using APIGasoline.Application.Features.RutasProcesadas.Notifications;

namespace APIGasoline.Application.Features.CargasCombustible.Commands;

public record DeleteCargaCombustibleCommand(int CargaCombustibleId) : IRequest;

public class DeleteCargaCombustibleCommandHandler : IRequestHandler<DeleteCargaCombustibleCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFileStorageService _storage;
    private readonly IPublisher _publisher;

    public DeleteCargaCombustibleCommandHandler(IUnitOfWork unitOfWork, IFileStorageService storage, IPublisher publisher)
    {
        _unitOfWork = unitOfWork;
        _storage = storage;
        _publisher = publisher;
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

        // Si el viaje ya está cerrado, el handler recalcula su rendimiento km/L
        await _publisher.Publish(new RutaViajeModificadaNotification(carga.ViajeId), cancellationToken);
    }
}
