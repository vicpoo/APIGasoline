using APIGasoline.Application.Features.Viajes.Common;
using APIGasoline.Application.Common.Security;
using APIGasoline.Application.Common.Interfaces;
using APIGasoline.Application.Common.Storage;
using APIGasoline.Application.Features.Viajes.DTOs;
using APIGasoline.Domain.Enums;
using APIGasoline.Domain.Interfaces;
using AutoMapper;
using MediatR;

namespace APIGasoline.Application.Features.Viajes.Queries;

/// <summary>
/// Lista los viajes del más reciente al más antiguo.
/// Filtros opcionales: empresa, carro, operador, solicitud de reserva y estado.
/// </summary>
public record GetViajesQuery(
    Guid? EmpresaId = null,
    Guid? CarroId = null,
    Guid? EmpleadoOperadorId = null,
    Guid? SolicitudReservaId = null,
    EstadoViaje? Estado = null) : IRequest<IReadOnlyList<ViajeDto>>;

public class GetViajesQueryHandler : IRequestHandler<GetViajesQuery, IReadOnlyList<ViajeDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IFileStorageService _storage;
    private readonly IUsuarioActual _usuarioActual;
    private readonly IDateTimeProvider _dateTime;

    public GetViajesQueryHandler(
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

    public async Task<IReadOnlyList<ViajeDto>> Handle(GetViajesQuery request, CancellationToken cancellationToken)
    {
        // Solo su empresa (salvo plataforma.multiempresa) y, sin viaje.ver_todos, solo sus viajes
        var empresaId = await _usuarioActual.ResolverEmpresaAsync(request.EmpresaId, cancellationToken);
        var empleadoOperadorId = await _usuarioActual.ResolverEmpleadoAsync(request.EmpleadoOperadorId, Permisos.ViajeVerTodos, cancellationToken);

        var viajes = await _unitOfWork.Viajes.GetAllAsync(
            empresaId, request.CarroId, empleadoOperadorId, request.SolicitudReservaId, request.Estado, cancellationToken);

        var dtos = await _mapper.Map<IReadOnlyList<ViajeDto>>(viajes).ConUrlsAsync(_storage, cancellationToken);
        var conPausas = await dtos.ConPausasAsync(_unitOfWork, _mapper, _dateTime.Now, cancellationToken);
        return await conPausas.ConOperadorAsync(_unitOfWork, _storage, cancellationToken);
    }
}
