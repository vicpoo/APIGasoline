using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Application.Common.Interfaces;
using APIGasoline.Domain.Interfaces;
using MediatR;

namespace APIGasoline.Application.Features.Carros.Commands;

public record DeleteCarroCommand(int CarroId) : IRequest;

public class DeleteCarroCommandHandler : IRequestHandler<DeleteCarroCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFileStorageService _storage;

    public DeleteCarroCommandHandler(IUnitOfWork unitOfWork, IFileStorageService storage)
    {
        _unitOfWork = unitOfWork;
        _storage = storage;
    }

    public async Task Handle(DeleteCarroCommand request, CancellationToken cancellationToken)
    {
        var carro = await _unitOfWork.Carros.GetByIdAsync(request.CarroId, cancellationToken)
            ?? throw new NotFoundException($"No existe un carro con el id {request.CarroId}.");

        if (await _unitOfWork.SolicitudesReserva.ExistsByCarroAsync(carro.CarroId, cancellationToken))
            throw new ConflictException("No se puede eliminar el carro porque tiene solicitudes de reserva registradas.");

        if (await _unitOfWork.Viajes.ExistsByCarroAsync(carro.CarroId, cancellationToken))
            throw new ConflictException("No se puede eliminar el carro porque tiene viajes registrados.");

        if (await _unitOfWork.CambiosEstatusCarro.ExistsByCarroAsync(carro.CarroId, cancellationToken))
            throw new ConflictException("No se puede eliminar el carro porque tiene historial de cambios de estatus.");

        var foto = carro.FotoVehiculo;

        _unitOfWork.Carros.Remove(carro);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // El registro ya se borró: si falla la limpieza del archivo solo se registra en el log
        await _storage.IntentarEliminarAsync(foto);
    }
}
