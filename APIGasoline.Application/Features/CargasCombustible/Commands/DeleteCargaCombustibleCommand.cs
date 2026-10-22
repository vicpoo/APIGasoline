using APIGasoline.Application.Common.Auditoria;
using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Application.Common.Interfaces;
using APIGasoline.Domain.Enums;
using APIGasoline.Domain.Interfaces;
using MediatR;
using APIGasoline.Application.Features.RutasProcesadas.Notifications;

namespace APIGasoline.Application.Features.CargasCombustible.Commands;

/// <summary>
/// Elimina una carga ya enviada. Sigue las mismas reglas que editarla (<see cref="ReglasCorreccion"/>):
/// motivo obligatorio; con el viaje en curso, el operador del viaje; con el viaje cerrado, solo quien
/// tenga viaje.corregir_cerrado. El borrado queda en la auditoría con su motivo.
/// </summary>
public record DeleteCargaCombustibleCommand(Guid CargaCombustibleId, string? Motivo) : IRequest;

public class DeleteCargaCombustibleCommandHandler : IRequestHandler<DeleteCargaCombustibleCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFileStorageService _storage;
    private readonly IPublisher _publisher;
    private readonly IUsuarioActual _usuarioActual;
    private readonly IContextoAuditoria _auditoria;

    public DeleteCargaCombustibleCommandHandler(
        IUnitOfWork unitOfWork,
        IFileStorageService storage,
        IPublisher publisher,
        IUsuarioActual usuarioActual,
        IContextoAuditoria auditoria)
    {
        _unitOfWork = unitOfWork;
        _storage = storage;
        _publisher = publisher;
        _usuarioActual = usuarioActual;
        _auditoria = auditoria;
    }

    public async Task Handle(DeleteCargaCombustibleCommand request, CancellationToken cancellationToken)
    {
        var carga = await _unitOfWork.CargasCombustible.GetByIdAsync(request.CargaCombustibleId, cancellationToken)
            ?? throw new NotFoundException($"No existe una carga de combustible con el id {request.CargaCombustibleId}.");

        var viaje = await _unitOfWork.Viajes.GetByIdAsync(carga.ViajeId, cancellationToken)
            ?? throw new NotFoundException($"No existe un viaje con el id {carga.ViajeId}.");

        // Quién puede eliminar y con qué motivo (mismas reglas que corregir)
        var motivo = await ReglasCorreccion.ValidarAsync(_usuarioActual, viaje, request.Motivo, cancellationToken);

        var comprobante = carga.FotoComprobante;

        // El interceptor guarda la fila borrada con esta acción y motivo
        _auditoria.Registrar(carga.CargaCombustibleId, AccionAuditoria.Eliminar, motivo);

        _unitOfWork.CargasCombustible.Remove(carga);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // El registro ya se borró: si falla la limpieza del archivo solo se registra en el log
        await _storage.IntentarEliminarAsync(comprobante);

        // Si el viaje ya está cerrado, el handler recalcula su rendimiento km/L
        await _publisher.Publish(new RutaViajeModificadaNotification(carga.ViajeId), cancellationToken);
    }
}
