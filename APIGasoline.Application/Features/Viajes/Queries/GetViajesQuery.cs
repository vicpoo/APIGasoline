using APIGasoline.Domain.Common;
using APIGasoline.Application.Features.Viajes.Common;
using APIGasoline.Application.Common.Security;
using APIGasoline.Application.Common.Interfaces;
using APIGasoline.Application.Common.Storage;
using APIGasoline.Application.Features.Viajes.DTOs;
using APIGasoline.Domain.Enums;
using APIGasoline.Domain.Interfaces;
using AutoMapper;
using APIGasoline.Application.Common.Messaging;

namespace APIGasoline.Application.Features.Viajes.Queries;

/// <summary>
/// Lista los viajes del más reciente al más antiguo.
/// Filtros opcionales: empresa, carro, operador, solicitud de reserva y estado.
/// </summary>
public sealed record GetViajesQuery(
    Guid? EmpresaId = null,
    Guid? CarroId = null,
    Guid? EmpleadoOperadorId = null,
    Guid? SolicitudReservaId = null,
    EstadoViaje? Estado = null,
    int? Pagina = null,
    int? TamanoPagina = null) : IQuery<PaginaDe<ViajeDto>>;

public sealed class GetViajesQueryHandler : IQueryHandler<GetViajesQuery, PaginaDe<ViajeDto>>
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

    public async Task<PaginaDe<ViajeDto>> Handle(GetViajesQuery request, CancellationToken cancellationToken)
    {
        // Solo su empresa (salvo plataforma.multiempresa) y, sin viaje.ver_todos, solo sus viajes
        var empresaId = await _usuarioActual.ResolverEmpresaAsync(request.EmpresaId, cancellationToken);
        var empleadoOperadorId = await _usuarioActual.ResolverEmpleadoAsync(request.EmpleadoOperadorId, Permisos.ViajeVerTodos, cancellationToken);

        var pagina = await _unitOfWork.Viajes.GetPaginaAsync(
            new Paginacion(request.Pagina, request.TamanoPagina),
            empresaId, request.CarroId, empleadoOperadorId, request.SolicitudReservaId, request.Estado, cancellationToken);

        // Fotos firmadas, pausas y operador: solo de la página que se devuelve
        var dtos = await _mapper.Map<IReadOnlyList<ViajeDto>>(pagina.Elementos).ConUrlsAsync(_storage, cancellationToken);
        var conPausas = await dtos.ConPausasAsync(_unitOfWork, _mapper, _dateTime.Now, cancellationToken);
        return pagina.Con(await conPausas.ConOperadorAsync(_unitOfWork, _storage, cancellationToken));
    }
}
