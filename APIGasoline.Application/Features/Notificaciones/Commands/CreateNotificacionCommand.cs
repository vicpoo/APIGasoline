using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Application.Features.Notificaciones.DTOs;
using APIGasoline.Domain.Entities;
using APIGasoline.Domain.Enums;
using APIGasoline.Domain.Interfaces;
using AutoMapper;
using MediatR;
using APIGasoline.Application.Common.Outbox;

namespace APIGasoline.Application.Features.Notificaciones.Commands;

/// <summary>
/// Alta manual de una notificación. Las de reservas y cambios de estatus
/// se generan automáticamente en sus propios flujos.
/// </summary>
public record CreateNotificacionCommand(
    Guid EmpleadoDestinatarioId,
    TipoNotificacion Tipo,
    Guid? ReferenciaId,
    string? Mensaje) : IRequest<NotificacionDto>;

public class CreateNotificacionCommandHandler : IRequestHandler<CreateNotificacionCommand, NotificacionDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly NotificadorPush _notificador;

    public CreateNotificacionCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, NotificadorPush notificador)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _notificador = notificador;
    }

    public async Task<NotificacionDto> Handle(CreateNotificacionCommand request, CancellationToken cancellationToken)
    {
        // La empresa se toma del empleado destinatario
        var empleado = await _unitOfWork.Empleados.GetByIdAsync(request.EmpleadoDestinatarioId, cancellationToken)
            ?? throw new NotFoundException($"No existe un empleado con el id {request.EmpleadoDestinatarioId}.");

        var notificacion = Notificacion.Crear(
            empleado.EmpresaId,
            empleado.EmpleadoId,
            request.Tipo,
            request.ReferenciaId,
            request.Mensaje);

        await _notificador.AgregarAsync(notificacion, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<NotificacionDto>(notificacion);
    }
}
