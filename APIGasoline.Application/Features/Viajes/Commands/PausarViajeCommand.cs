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
/// Pone en pausa un viaje en curso (comida, carga de gasolina, espera...).
/// Si no se manda fecha, se usa la hora actual. Las reglas (en curso, sin otra pausa abierta, fechas)
/// las valida el dominio en <see cref="Domain.Entities.Viaje.Pausar"/>.
/// </summary>
public record PausarViajeCommand(
    Guid ViajeId,
    DateTime? Fecha,
    string? Motivo,
    decimal? UbicacionLat,
    decimal? UbicacionLng) : IRequest<ViajeDto>;

public class PausarViajeCommandHandler : IRequestHandler<PausarViajeCommand, ViajeDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IFileStorageService _storage;
    private readonly IDateTimeProvider _dateTime;
    private readonly IUsuarioActual _usuarioActual;

    public PausarViajeCommandHandler(
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

    public async Task<ViajeDto> Handle(PausarViajeCommand request, CancellationToken cancellationToken)
    {
        var viaje = await _unitOfWork.Viajes.GetByIdAsync(request.ViajeId, cancellationToken)
            ?? throw new NotFoundException($"No existe un viaje con el id {request.ViajeId}.");

        await AccesoViaje.ValidarOperadorAsync(_usuarioActual, viaje, cancellationToken);

        var ultimaPausa = await _unitOfWork.ViajePausas.GetUltimaByViajeAsync(viaje.ViajeId, cancellationToken);
        var ahora = _dateTime.Now;

        var pausa = viaje.Pausar(ultimaPausa, request.Fecha ?? ahora, request.Motivo, request.UbicacionLat, request.UbicacionLng);

        await _unitOfWork.ViajePausas.AddAsync(pausa, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var dto = await _mapper.Map<ViajeDto>(viaje).ConUrlsAsync(_storage, cancellationToken);
        return await dto.ConPausasAsync(_unitOfWork, _mapper, ahora, cancellationToken);
    }
}
