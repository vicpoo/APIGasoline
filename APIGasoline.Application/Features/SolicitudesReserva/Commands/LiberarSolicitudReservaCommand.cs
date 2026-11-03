using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Application.Common.Interfaces;
using APIGasoline.Application.Common.Security;
using APIGasoline.Application.Features.SolicitudesReserva.Common;
using APIGasoline.Application.Features.SolicitudesReserva.DTOs;
using APIGasoline.Domain.Interfaces;
using AutoMapper;
using MediatR;

namespace APIGasoline.Application.Features.SolicitudesReserva.Commands;

/// <summary>
/// HU-USO-04: el operador terminó todos sus viajes con el carro y lo libera ("Dejar carro" en la app).
/// La reserva pasa a "finalizada" (con fecha y observaciones) y el carro vuelve a "disponible";
/// el cambio queda en la bitácora de estatus como "liberacion".
/// Cada quien libera solo SU reserva; la de otro empleado, solo con asignacion.gestionar
/// (para eso la app usa "Desvincular", que además pide motivo y avisa al empleado).
/// </summary>
/// <param name="FechaLiberacion">Hora del celular (puede registrarse offline). Si no viene, la del servidor.</param>
public record LiberarSolicitudReservaCommand(
    Guid SolicitudReservaId,
    DateTime? FechaLiberacion,
    string? Observaciones) : IRequest<SolicitudReservaDto>;

public class LiberarSolicitudReservaCommandHandler : IRequestHandler<LiberarSolicitudReservaCommand, SolicitudReservaDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IDateTimeProvider _dateTime;
    private readonly IUsuarioActual _usuarioActual;

    public LiberarSolicitudReservaCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, IDateTimeProvider dateTime, IUsuarioActual usuarioActual)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _dateTime = dateTime;
        _usuarioActual = usuarioActual;
    }

    public async Task<SolicitudReservaDto> Handle(LiberarSolicitudReservaCommand request, CancellationToken cancellationToken)
    {
        var solicitud = await _unitOfWork.SolicitudesReserva.GetByIdAsync(request.SolicitudReservaId, cancellationToken)
            ?? throw new NotFoundException($"No existe una solicitud de reserva con el id {request.SolicitudReservaId}.");

        // Solo su empresa, y solo su propia reserva salvo que tenga asignacion.gestionar
        await _usuarioActual.ValidarEmpresaAsync(solicitud.EmpresaId, cancellationToken);
        await _usuarioActual.ValidarPropioOPrivilegioAsync(
            solicitud.EmpleadoSolicitanteId == _usuarioActual.EmpleadoId,
            Permisos.AsignacionGestionar,
            cancellationToken);

        var fecha = request.FechaLiberacion ?? _dateTime.Now;

        // La bitácora queda a nombre del solicitante (es quien deja el carro)
        await LiberacionReserva.LiberarAsync(
            _unitOfWork,
            solicitud,
            solicitud.EmpleadoSolicitanteId,
            fecha,
            request.Observaciones,
            motivoBitacora: null,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<SolicitudReservaDto>(solicitud);
    }
}
