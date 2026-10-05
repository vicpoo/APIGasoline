using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Domain.Interfaces;
using MediatR;

namespace APIGasoline.Application.Features.SolicitudesReserva.Commands;

/// <summary>
/// Borra físicamente la solicitud. No se permite si está aprobada (el carro está en uso).
/// </summary>
public record DeleteSolicitudReservaCommand(int SolicitudReservaId) : IRequest;

public class DeleteSolicitudReservaCommandHandler : IRequestHandler<DeleteSolicitudReservaCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteSolicitudReservaCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(DeleteSolicitudReservaCommand request, CancellationToken cancellationToken)
    {
        var solicitud = await _unitOfWork.SolicitudesReserva.GetByIdAsync(request.SolicitudReservaId, cancellationToken)
            ?? throw new NotFoundException($"No existe una solicitud de reserva con el id {request.SolicitudReservaId}.");

        if (solicitud.EstaAprobada)
            throw new ConflictException("No se puede eliminar una solicitud aprobada. Cancélala primero para liberar el carro.");

        if (await _unitOfWork.Viajes.ExistsBySolicitudAsync(solicitud.SolicitudReservaId, cancellationToken))
            throw new ConflictException("No se puede eliminar la solicitud porque tiene viajes registrados.");

        _unitOfWork.SolicitudesReserva.Remove(solicitud);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
