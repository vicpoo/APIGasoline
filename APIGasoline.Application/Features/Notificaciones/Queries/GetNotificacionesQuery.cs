using APIGasoline.Application.Common.Interfaces;
using APIGasoline.Application.Common.Security;
using APIGasoline.Application.Features.Notificaciones.DTOs;
using APIGasoline.Domain.Enums;
using APIGasoline.Domain.Interfaces;
using AutoMapper;
using MediatR;

namespace APIGasoline.Application.Features.Notificaciones.Queries;

/// <summary>
/// Bandeja de la más reciente a la más antigua.
/// </summary>
public record GetNotificacionesQuery(
    Guid? EmpresaId = null,
    Guid? EmpleadoDestinatarioId = null,
    bool SoloNoLeidas = false,
    TipoNotificacion? Tipo = null) : IRequest<IReadOnlyList<NotificacionDto>>;

public class GetNotificacionesQueryHandler : IRequestHandler<GetNotificacionesQuery, IReadOnlyList<NotificacionDto>>
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

    public async Task<IReadOnlyList<NotificacionDto>> Handle(GetNotificacionesQuery request, CancellationToken cancellationToken)
    {
        // Cada quien ve su bandeja; con notificacion.enviar, también las de otros (de su empresa)
        var empresaId = await _usuarioActual.ResolverEmpresaAsync(request.EmpresaId, cancellationToken);
        var empleadoDestinatarioId = await _usuarioActual.ResolverEmpleadoAsync(request.EmpleadoDestinatarioId, Permisos.NotificacionEnviar, cancellationToken);

        var notificaciones = await _unitOfWork.Notificaciones.GetAllAsync(
            empresaId, empleadoDestinatarioId, request.SoloNoLeidas, request.Tipo, cancellationToken);

        return _mapper.Map<IReadOnlyList<NotificacionDto>>(notificaciones);
    }
}
