using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Application.Common.Interfaces;
using APIGasoline.Application.Common.Storage;
using APIGasoline.Application.Features.Viajes.Common;
using APIGasoline.Application.Features.Viajes.DTOs;
using APIGasoline.Domain.Interfaces;
using AutoMapper;
using MediatR;

namespace APIGasoline.Application.Features.Viajes.Commands;

/// <summary>
/// Reanuda un viaje en pausa (cierra la pausa abierta). Si no se manda fecha, se usa la hora actual.
/// </summary>
public record ReanudarViajeCommand(Guid ViajeId, DateTime? Fecha) : IRequest<ViajeDto>;

public class ReanudarViajeCommandHandler : IRequestHandler<ReanudarViajeCommand, ViajeDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IFileStorageService _storage;
    private readonly IDateTimeProvider _dateTime;
    private readonly IUsuarioActual _usuarioActual;

    public ReanudarViajeCommandHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IFileStorageService storage,
        IDateTimeProvider dateTime,
        IUsuarioActual usuarioActual)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _storage = storage;
        _dateTime = dateTime;
        _usuarioActual = usuarioActual;
    }

    public async Task<ViajeDto> Handle(ReanudarViajeCommand request, CancellationToken cancellationToken)
    {
        var viaje = await _unitOfWork.Viajes.GetByIdAsync(request.ViajeId, cancellationToken)
            ?? throw new NotFoundException($"No existe un viaje con el id {request.ViajeId}.");

        await AccesoViaje.ValidarOperadorAsync(_usuarioActual, viaje, cancellationToken);

        var pausaAbierta = await _unitOfWork.ViajePausas.GetAbiertaByViajeAsync(viaje.ViajeId, cancellationToken);
        var ahora = _dateTime.Now;

        viaje.Reanudar(pausaAbierta, request.Fecha ?? ahora);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var dto = await _mapper.Map<ViajeDto>(viaje).ConUrlsAsync(_storage, cancellationToken);
        return await dto.ConPausasAsync(_unitOfWork, _mapper, ahora, cancellationToken);
    }
}
