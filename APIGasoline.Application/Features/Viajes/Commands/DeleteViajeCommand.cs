using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Domain.Interfaces;
using MediatR;

namespace APIGasoline.Application.Features.Viajes.Commands;

/// <summary>
/// Borra físicamente el viaje. No se permite mientras está en curso.
/// </summary>
public record DeleteViajeCommand(Guid ViajeId) : IRequest;

public class DeleteViajeCommandHandler : IRequestHandler<DeleteViajeCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteViajeCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(DeleteViajeCommand request, CancellationToken cancellationToken)
    {
        var viaje = await _unitOfWork.Viajes.GetByIdAsync(request.ViajeId, cancellationToken)
            ?? throw new NotFoundException($"No existe un viaje con el id {request.ViajeId}.");

        if (viaje.EstaEnCurso)
            throw new ConflictException("No se puede eliminar un viaje en curso. Finalízalo o interrúmpelo primero.");

        if (await _unitOfWork.CargasCombustible.ExistsByViajeAsync(viaje.ViajeId, cancellationToken))
            throw new ConflictException("No se puede eliminar el viaje porque tiene cargas de combustible registradas.");

        if (await _unitOfWork.CambiosEstatusCarro.ExistsByViajeAsync(viaje.ViajeId, cancellationToken))
            throw new ConflictException("No se puede eliminar el viaje porque aparece en la bitácora de estatus del carro.");

        _unitOfWork.Viajes.Remove(viaje);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
