using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Application.Common.Interfaces;
using APIGasoline.Application.Common.Security;
using APIGasoline.Application.Features.Notificaciones.DTOs;
using APIGasoline.Domain.Interfaces;
using AutoMapper;
using APIGasoline.Application.Common.Messaging;

namespace APIGasoline.Application.Features.Notificaciones.Queries;

public sealed record GetNotificacionByIdQuery(Guid NotificacionId) : IQuery<NotificacionDto>;

public sealed class GetNotificacionByIdQueryHandler : IQueryHandler<GetNotificacionByIdQuery, NotificacionDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IUsuarioActual _usuarioActual;

    public GetNotificacionByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, IUsuarioActual usuarioActual)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _usuarioActual = usuarioActual;
    }

    public async Task<NotificacionDto> Handle(GetNotificacionByIdQuery request, CancellationToken cancellationToken)
    {
        var notificacion = await _unitOfWork.Notificaciones.GetByIdAsync(request.NotificacionId, cancellationToken)
            ?? throw new NotFoundException($"No existe una notificación con el id {request.NotificacionId}.");

        // Cada quien ve y maneja sus notificaciones; las de otro empleado, solo quien envía notificaciones
        await _usuarioActual.ValidarPropioOAlgunPrivilegioAsync(
            notificacion.EmpleadoDestinatarioId == _usuarioActual.EmpleadoId,
            [Permisos.NotificacionEnviar],
            "Solo puedes ver y manejar tus propias notificaciones",
            cancellationToken);

        return _mapper.Map<NotificacionDto>(notificacion);
    }
}
