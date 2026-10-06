using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Application.Common.Interfaces;
using APIGasoline.Domain.Interfaces;
using MediatR;

namespace APIGasoline.Application.Features.Carros.Commands;

/// <summary>
/// Quita la foto del vehículo: limpia la llave en BD y después elimina el archivo.
/// </summary>
public record EliminarFotoVehiculoCommand(int CarroId) : IRequest;

public class EliminarFotoVehiculoCommandHandler : IRequestHandler<EliminarFotoVehiculoCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFileStorageService _storage;

    public EliminarFotoVehiculoCommandHandler(IUnitOfWork unitOfWork, IFileStorageService storage)
    {
        _unitOfWork = unitOfWork;
        _storage = storage;
    }

    public async Task Handle(EliminarFotoVehiculoCommand request, CancellationToken cancellationToken)
    {
        var carro = await _unitOfWork.Carros.GetByIdAsync(request.CarroId, cancellationToken)
            ?? throw new NotFoundException($"No existe un carro con el id {request.CarroId}.");

        if (carro.FotoVehiculo is null)
            throw new NotFoundException("El carro no tiene foto.");

        var foto = carro.FotoVehiculo;

        carro.CambiarFotoVehiculo(null);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _storage.IntentarEliminarAsync(foto);
    }
}
