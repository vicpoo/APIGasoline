using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Application.Common.Interfaces;
using APIGasoline.Application.Common.Storage;
using APIGasoline.Application.Features.Carros.DTOs;
using APIGasoline.Domain.Enums;
using APIGasoline.Domain.Interfaces;
using AutoMapper;
using MediatR;

namespace APIGasoline.Application.Features.Carros.Commands;

/// <summary>
/// HU-AST-03: retira el carro de la flota. No se puede dar de baja un carro en uso.
/// El carro no se borra: su historial de viajes y cargas se conserva.
/// Las solicitudes pendientes del carro se cancelan.
/// </summary>
public record DarDeBajaCarroCommand(Guid CarroId, DateTime? FechaBaja, string Motivo) : IRequest<CarroDto>;

public class DarDeBajaCarroCommandHandler : IRequestHandler<DarDeBajaCarroCommand, CarroDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IDateTimeProvider _dateTime;
    private readonly IFileStorageService _storage;

    public DarDeBajaCarroCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, IDateTimeProvider dateTime, IFileStorageService storage)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _dateTime = dateTime;
        _storage = storage;
    }

    public async Task<CarroDto> Handle(DarDeBajaCarroCommand request, CancellationToken cancellationToken)
    {
        var carro = await _unitOfWork.Carros.GetByIdAsync(request.CarroId, cancellationToken)
            ?? throw new NotFoundException($"No existe un carro con el id {request.CarroId}.");

        if (await _unitOfWork.Viajes.ExisteEnCursoPorCarroAsync(carro.CarroId, cancellationToken))
            throw new ConflictException("No se puede dar de baja el carro porque tiene un viaje en curso.");

        if (await _unitOfWork.SolicitudesReserva.GetAprobadaByCarroAsync(carro.CarroId, cancellationToken) is not null)
            throw new ConflictException("No se puede dar de baja el carro porque está asignado. Primero debe liberarse.");

        var ahora = _dateTime.Now;

        // El dominio valida el motivo y que el carro no esté en uso
        carro.DarDeBaja(request.FechaBaja ?? ahora, request.Motivo);

        // Las solicitudes en espera ya no se podrán aprobar
        var pendientes = await _unitOfWork.SolicitudesReserva.GetPendientesByCarroAsync(carro.CarroId, Guid.Empty, cancellationToken);

        foreach (var pendiente in pendientes)
            pendiente.Cancelar(ahora, "El carro fue dado de baja.");

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await _mapper.Map<CarroDto>(carro).ConUrlsAsync(_storage, cancellationToken);
    }
}
