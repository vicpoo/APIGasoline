using APIGasoline.Application.Common.Interfaces;
using APIGasoline.Application.Common.Security;
using APIGasoline.Application.Common.Sesiones;
using APIGasoline.Domain.Enums;
using APIGasoline.Domain.Interfaces;
using MediatR;

namespace APIGasoline.Application.Features.Sesiones.Commands;

/// <summary>
/// Cierra todas las sesiones abiertas de un usuario. La propia, o la de cualquiera con usuario.gestionar.
/// Con <see cref="ConservarActual"/> = true no se cierra la sesión desde la que se hace la petición
/// ("cerrar sesión en todos los demás dispositivos"). Devuelve cuántas cerró.
/// </summary>
public record CerrarSesionesUsuarioCommand(Guid UsuarioId, bool ConservarActual) : IRequest<int>, IRecursoPropio
{
    public bool EsPropio(IUsuarioActual usuario) => UsuarioId == usuario.UsuarioId;

    public string PrivilegioParaOtros => Permisos.UsuarioGestionar;
}

public class CerrarSesionesUsuarioCommandHandler : IRequestHandler<CerrarSesionesUsuarioCommand, int>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUsuarioActual _usuarioActual;
    private readonly IDateTimeProvider _dateTime;

    public CerrarSesionesUsuarioCommandHandler(IUnitOfWork unitOfWork, IUsuarioActual usuarioActual, IDateTimeProvider dateTime)
    {
        _unitOfWork = unitOfWork;
        _usuarioActual = usuarioActual;
        _dateTime = dateTime;
    }

    public async Task<int> Handle(CerrarSesionesUsuarioCommand request, CancellationToken cancellationToken)
    {
        var esPropio = request.UsuarioId == _usuarioActual.UsuarioId;

        var motivo = esPropio
            ? MotivoRevocacionSesion.CerradaPorUsuario
            : MotivoRevocacionSesion.CerradaPorAdministrador;

        var excepto = esPropio && request.ConservarActual ? _usuarioActual.SesionId : null;

        var cerradas = await _unitOfWork.RevocarDeUsuarioAsync(
            request.UsuarioId, motivo, _dateTime.Now, _usuarioActual.UsuarioId, excepto, cancellationToken);

        if (cerradas > 0)
            await _unitOfWork.SaveChangesAsync(cancellationToken);

        return cerradas;
    }
}
