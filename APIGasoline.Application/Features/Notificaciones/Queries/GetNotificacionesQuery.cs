using APIGasoline.Domain.Common;
using APIGasoline.Application.Common.Interfaces;
using APIGasoline.Application.Common.Security;
using APIGasoline.Application.Features.Notificaciones.DTOs;
using APIGasoline.Domain.Enums;
using APIGasoline.Domain.Interfaces;
using AutoMapper;
using APIGasoline.Application.Common.Messaging;

namespace APIGasoline.Application.Features.Notificaciones.Queries;

/// <summary>
/// Bandeja de la más reciente a la más antigua.
/// </summary>
public sealed record GetNotificacionesQuery(
    Guid? EmpresaId = null,
    Guid? EmpleadoDestinatarioId = null,
    bool SoloNoLeidas = false,
    TipoNotificacion? Tipo = null,
    int? Pagina = null,
    int? TamanoPagina = null) : IQuery<PaginaDe<NotificacionDto>>;

public sealed class GetNotificacionesQueryHandler : IQueryHandler<GetNotificacionesQuery, PaginaDe<NotificacionDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IUsuarioActual _usuarioActual;

    public GetNotificacionesQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, IUsuarioActual usuarioActual)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _usuarioActual = usuarioActual;
    }

    public async Task<PaginaDe<NotificacionDto>> Handle(GetNotificacionesQuery request, CancellationToken cancellationToken)
    {
        // Cada quien ve su bandeja; con notificacion.enviar, también las de otros (de su empresa)
        var empresaId = await _usuarioActual.ResolverEmpresaAsync(request.EmpresaId, cancellationToken);
        var empleadoDestinatarioId = await _usuarioActual.ResolverEmpleadoAsync(request.EmpleadoDestinatarioId, Permisos.NotificacionEnviar, cancellationToken);

        var pagina = await _unitOfWork.Notificaciones.GetPaginaAsync(
            new Paginacion(request.Pagina, request.TamanoPagina),
            empresaId, empleadoDestinatarioId, request.SoloNoLeidas, request.Tipo, cancellationToken);

        return pagina.Con(_mapper.Map<IReadOnlyList<NotificacionDto>>(pagina.Elementos));
    }
}
