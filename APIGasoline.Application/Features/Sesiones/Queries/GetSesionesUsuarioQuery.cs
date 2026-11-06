using APIGasoline.Application.Common.Interfaces;
using APIGasoline.Application.Common.Security;
using APIGasoline.Application.Features.Sesiones.DTOs;
using APIGasoline.Domain.Interfaces;
using MediatR;

namespace APIGasoline.Application.Features.Sesiones.Queries;

/// <summary>
/// Sesiones abiertas de un usuario. Las propias, o las de cualquiera con usuario.gestionar.
/// </summary>
public record GetSesionesUsuarioQuery(Guid UsuarioId) : IRequest<IReadOnlyList<SesionUsuarioDto>>, IRecursoPropio
{
    public bool EsPropio(IUsuarioActual usuario) => UsuarioId == usuario.UsuarioId;

    public string PrivilegioParaOtros => Permisos.UsuarioGestionar;
}

public class GetSesionesUsuarioQueryHandler : IRequestHandler<GetSesionesUsuarioQuery, IReadOnlyList<SesionUsuarioDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUsuarioActual _usuarioActual;
    private readonly IDateTimeProvider _dateTime;

    public GetSesionesUsuarioQueryHandler(IUnitOfWork unitOfWork, IUsuarioActual usuarioActual, IDateTimeProvider dateTime)
    {
        _unitOfWork = unitOfWork;
        _usuarioActual = usuarioActual;
        _dateTime = dateTime;
    }

    public async Task<IReadOnlyList<SesionUsuarioDto>> Handle(GetSesionesUsuarioQuery request, CancellationToken cancellationToken)
    {
        var sesiones = await _unitOfWork.Sesiones.GetActivasByUsuarioAsync(request.UsuarioId, _dateTime.Now, cancellationToken);
        var sesionActual = _usuarioActual.SesionId;

        return sesiones
            .Select(s => new SesionUsuarioDto(
                s.SesionUsuarioId,
                s.Plataforma,
                s.NombreDispositivo,
                s.Ip,
                s.UserAgent,
                s.FechaCreacion,
                s.FechaUltimoUso,
                s.ExpiraEn,
                s.SesionUsuarioId == sesionActual))
            .ToList();
    }
}
