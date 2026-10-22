using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Application.Common.Interfaces;
using APIGasoline.Application.Common.Security;
using APIGasoline.Application.Features.SolicitudesReserva.Common;
using APIGasoline.Application.Features.SolicitudesReserva.DTOs;
using APIGasoline.Domain.Interfaces;
using MediatR;

namespace APIGasoline.Application.Features.SolicitudesReserva.Queries;

/// <summary>
/// Detalle de una solicitud: motivo, solicitante (foto y licencia), carro y confirmador.
/// </summary>
public record GetSolicitudReservaByIdQuery(Guid SolicitudReservaId) : IRequest<SolicitudReservaDetalleDto>;

public class GetSolicitudReservaByIdQueryHandler : IRequestHandler<GetSolicitudReservaByIdQuery, SolicitudReservaDetalleDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFileStorageService _storage;
    private readonly IUsuarioActual _usuarioActual;

    public GetSolicitudReservaByIdQueryHandler(IUnitOfWork unitOfWork, IFileStorageService storage, IUsuarioActual usuarioActual)
    {
        _unitOfWork = unitOfWork;
        _storage = storage;
        _usuarioActual = usuarioActual;
    }

    public async Task<SolicitudReservaDetalleDto> Handle(GetSolicitudReservaByIdQuery request, CancellationToken cancellationToken)
    {
        var solicitud = await _unitOfWork.SolicitudesReserva.GetByIdAsync(request.SolicitudReservaId, cancellationToken)
            ?? throw new NotFoundException($"No existe una solicitud de reserva con el id {request.SolicitudReservaId}.");

        // De su empresa y, sin solicitud.ver_todas, solo las suyas
        await _usuarioActual.ValidarEmpresaAsync(solicitud.EmpresaId, cancellationToken);
        await _usuarioActual.ValidarPropioOPrivilegioAsync(
            solicitud.EmpleadoSolicitanteId == _usuarioActual.EmpleadoId, Permisos.SolicitudVerTodas, cancellationToken);

        return await DatosSolicitud.DetalleAsync(solicitud, _unitOfWork, _storage, cancellationToken);
    }
}
