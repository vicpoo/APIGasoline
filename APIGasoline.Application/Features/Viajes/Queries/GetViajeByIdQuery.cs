using APIGasoline.Application.Features.Viajes.Common;
using APIGasoline.Application.Common.Interfaces;
using APIGasoline.Application.Common.Storage;
using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Application.Features.Viajes.DTOs;
using APIGasoline.Domain.Interfaces;
using AutoMapper;
using MediatR;

namespace APIGasoline.Application.Features.Viajes.Queries;

public record GetViajeByIdQuery(Guid ViajeId) : IRequest<ViajeDto>;

public class GetViajeByIdQueryHandler : IRequestHandler<GetViajeByIdQuery, ViajeDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IFileStorageService _storage;
    private readonly IUsuarioActual _usuarioActual;
    private readonly IDateTimeProvider _dateTime;

    public GetViajeByIdQueryHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IFileStorageService storage,
        IUsuarioActual usuarioActual,
        IDateTimeProvider dateTime)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _storage = storage;
        _usuarioActual = usuarioActual;
        _dateTime = dateTime;
    }

    public async Task<ViajeDto> Handle(GetViajeByIdQuery request, CancellationToken cancellationToken)
    {
        var viaje = await _unitOfWork.Viajes.GetByIdAsync(request.ViajeId, cancellationToken)
            ?? throw new NotFoundException($"No existe un viaje con el id {request.ViajeId}.");

        // De su empresa y, sin viaje.ver_todos, solo sus propios viajes
        await AccesoViaje.ValidarLecturaAsync(_usuarioActual, viaje, cancellationToken);

        var dto = await _mapper.Map<ViajeDto>(viaje).ConUrlsAsync(_storage, cancellationToken);
        var conPausas = await dto.ConPausasAsync(_unitOfWork, _mapper, _dateTime.Now, cancellationToken);
        return await conPausas.ConOperadorAsync(_unitOfWork, _storage, cancellationToken);
    }
}
