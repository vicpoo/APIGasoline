using APIGasoline.Application.Common.Interfaces;
using APIGasoline.Application.Common.Storage;
using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Application.Common.Security;
using APIGasoline.Application.Features.Viajes.Common;
using APIGasoline.Application.Features.CargasCombustible.DTOs;
using APIGasoline.Domain.Interfaces;
using AutoMapper;
using APIGasoline.Application.Common.Messaging;

namespace APIGasoline.Application.Features.CargasCombustible.Queries;

public sealed record GetCargaCombustibleByIdQuery(Guid CargaCombustibleId) : IQuery<CargaCombustibleDto>;

public sealed class GetCargaCombustibleByIdQueryHandler : IQueryHandler<GetCargaCombustibleByIdQuery, CargaCombustibleDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IFileStorageService _storage;
    private readonly IUsuarioActual _usuarioActual;

    public GetCargaCombustibleByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, IFileStorageService storage, IUsuarioActual usuarioActual)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _storage = storage;
        _usuarioActual = usuarioActual;
    }

    public async Task<CargaCombustibleDto> Handle(GetCargaCombustibleByIdQuery request, CancellationToken cancellationToken)
    {
        var carga = await _unitOfWork.CargasCombustible.GetByIdAsync(request.CargaCombustibleId, cancellationToken)
            ?? throw new NotFoundException($"No existe una carga de combustible con el id {request.CargaCombustibleId}.");

        var viaje = await _unitOfWork.Viajes.GetByIdAsync(carga.ViajeId, cancellationToken)
            ?? throw new NotFoundException($"No existe una carga de combustible con el id {request.CargaCombustibleId}.");

        // La carga es del operador del viaje; también la ven quienes la corrigen o la eliminan
        await AccesoViaje.ValidarLecturaDetalleAsync(_usuarioActual, viaje, cancellationToken, Permisos.CargaEditar, Permisos.CargaEliminar);

        return await _mapper.Map<CargaCombustibleDto>(carga).ConUrlsAsync(_storage, cancellationToken);
    }
}
