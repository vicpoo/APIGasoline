using APIGasoline.Application.Common.Interfaces;
using APIGasoline.Application.Common.Security;
using APIGasoline.Application.Features.Asignaciones.Common;
using APIGasoline.Application.Features.Asignaciones.DTOs;
using APIGasoline.Domain.Enums;
using APIGasoline.Domain.Interfaces;
using MediatR;

namespace APIGasoline.Application.Features.Asignaciones.Queries;

/// <summary>
/// Vista Asignaciones (asignacion.gestionar): solo lo vinculado (carros con quién los tiene y
/// empleados con sus carros), en una sola llamada. Solo la empresa del token, salvo plataforma.multiempresa.
/// </summary>
public record GetAsignacionesQuery(Guid? EmpresaId = null) : IRequest<AsignacionesDto>;

public class GetAsignacionesQueryHandler : IRequestHandler<GetAsignacionesQuery, AsignacionesDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUsuarioActual _usuarioActual;
    private readonly IFileStorageService _storage;

    public GetAsignacionesQueryHandler(IUnitOfWork unitOfWork, IUsuarioActual usuarioActual, IFileStorageService storage)
    {
        _unitOfWork = unitOfWork;
        _usuarioActual = usuarioActual;
        _storage = storage;
    }

    public async Task<AsignacionesDto> Handle(GetAsignacionesQuery request, CancellationToken cancellationToken)
    {
        var empresaId = await _usuarioActual.ResolverEmpresaAsync(request.EmpresaId, cancellationToken);

        // 4 consultas para toda la vista (sin N+1)
        var carros = await _unitOfWork.Carros.GetAllAsync(empresaId, cancellationToken: cancellationToken);
        var empleados = await _unitOfWork.Empleados.GetAllAsync(empresaId, cancellationToken);
        var aprobadas = await _unitOfWork.SolicitudesReserva.GetAllAsync(
            empresaId, estado: EstadoSolicitudReserva.Aprobada, cancellationToken: cancellationToken);
        var viajesEnCurso = await _unitOfWork.Viajes.GetAllAsync(
            empresaId, estado: EstadoViaje.EnCurso, cancellationToken: cancellationToken);

        var conViajeEnCurso = viajesEnCurso
            .Where(v => v.SolicitudReservaId is not null)
            .Select(v => v.SolicitudReservaId!.Value)
            .ToHashSet();

        // Las URLs de las fotos se firman una vez por llave
        var llaves = carros.Select(c => c.FotoVehiculo)
            .Concat(empleados.Select(e => e.FotoPerfil))
            .Where(l => !string.IsNullOrWhiteSpace(l))
            .Distinct()
            .ToList();

        var urls = new Dictionary<string, string?>();
        foreach (var llave in llaves)
            urls[llave!] = await _storage.ObtenerUrlAsync(llave!, cancellationToken);

        return ArmadoAsignaciones.Armar(
            carros,
            empleados,
            aprobadas,
            conViajeEnCurso,
            llave => llave is not null && urls.TryGetValue(llave, out var url) ? url : null);
    }
}
