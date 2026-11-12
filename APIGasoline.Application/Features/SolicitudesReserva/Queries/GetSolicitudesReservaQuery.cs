using APIGasoline.Domain.Common;
using APIGasoline.Application.Common.Interfaces;
using APIGasoline.Application.Common.Security;
using APIGasoline.Application.Features.SolicitudesReserva.Common;
using APIGasoline.Application.Features.SolicitudesReserva.DTOs;
using APIGasoline.Domain.Enums;
using APIGasoline.Domain.Interfaces;
using AutoMapper;
using APIGasoline.Application.Common.Messaging;

namespace APIGasoline.Application.Features.SolicitudesReserva.Queries;

/// <summary>
/// Lista las solicitudes de la más antigua a la más reciente (cola de confirmación).
/// Filtros opcionales: empresa, carro, empleado solicitante y estado.
/// Cada solicitud trae el nombre y la foto del solicitante y el carro (para la tarjeta).
/// </summary>
public sealed record GetSolicitudesReservaQuery(
    Guid? EmpresaId = null,
    Guid? CarroId = null,
    Guid? EmpleadoSolicitanteId = null,
    EstadoSolicitudReserva? Estado = null,
    int? Pagina = null,
    int? TamanoPagina = null) : IQuery<PaginaDe<SolicitudReservaDto>>;

public sealed class GetSolicitudesReservaQueryHandler : IQueryHandler<GetSolicitudesReservaQuery, PaginaDe<SolicitudReservaDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IUsuarioActual _usuarioActual;
    private readonly IFileStorageService _storage;

    public GetSolicitudesReservaQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, IUsuarioActual usuarioActual, IFileStorageService storage)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _usuarioActual = usuarioActual;
        _storage = storage;
    }

    public async Task<PaginaDe<SolicitudReservaDto>> Handle(GetSolicitudesReservaQuery request, CancellationToken cancellationToken)
    {
        // Solo su empresa y, sin solicitud.ver_todas, solo sus propias solicitudes
        var empresaId = await _usuarioActual.ResolverEmpresaAsync(request.EmpresaId, cancellationToken);
        var empleadoSolicitanteId = await _usuarioActual.ResolverEmpleadoAsync(request.EmpleadoSolicitanteId, Permisos.SolicitudVerTodas, cancellationToken);

        var pagina = await _unitOfWork.SolicitudesReserva.GetPaginaAsync(
            new Paginacion(request.Pagina, request.TamanoPagina),
            empresaId, request.CarroId, empleadoSolicitanteId, request.Estado, cancellationToken);

        return pagina.Con(await _mapper.Map<IReadOnlyList<SolicitudReservaDto>>(pagina.Elementos)
            .ConDatosAsync(_unitOfWork, _storage, cancellationToken));
    }
}
