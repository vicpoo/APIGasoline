using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Application.Common.Interfaces;
using APIGasoline.Application.Common.Security;
using APIGasoline.Domain.Enums;
using APIGasoline.Domain.Interfaces;
using APIGasoline.Application.Common.Messaging;

namespace APIGasoline.Application.Features.Sesiones.Commands;

/// <summary>
/// Cierra UNA sesión de un usuario (por ejemplo, "cerrar sesión en el celular que perdí").
/// La propia, o la de cualquiera con usuario.gestionar. El dispositivo ya no podrá renovar su token;
/// su JWT de acceso deja de servir cuando expire (máximo Jwt:ExpiracionMinutos).
/// </summary>
public sealed record CerrarSesionUsuarioCommand(Guid UsuarioId, Guid SesionId) : ICommand, IRecursoPropio
{
    public bool EsPropio(IUsuarioActual usuario) => UsuarioId == usuario.UsuarioId;

    public string PrivilegioParaOtros => Permisos.UsuarioGestionar;
}

public sealed class CerrarSesionUsuarioCommandHandler : ICommandHandler<CerrarSesionUsuarioCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUsuarioActual _usuarioActual;
    private readonly IDateTimeProvider _dateTime;

    public CerrarSesionUsuarioCommandHandler(IUnitOfWork unitOfWork, IUsuarioActual usuarioActual, IDateTimeProvider dateTime)
    {
        _unitOfWork = unitOfWork;
        _usuarioActual = usuarioActual;
        _dateTime = dateTime;
    }

    public async Task Handle(CerrarSesionUsuarioCommand request, CancellationToken cancellationToken)
    {
        // El usuario de otra empresa "no existe" (filtro por empresa). Las sesiones no tienen filtro propio.
        if (request.UsuarioId != _usuarioActual.UsuarioId && !await _unitOfWork.Usuarios.ExistsAsync(request.UsuarioId, cancellationToken))
            throw new NotFoundException($"No existe un usuario con el id {request.UsuarioId}.");

        var sesion = await _unitOfWork.Sesiones.GetByIdAsync(request.SesionId, cancellationToken);

        // Si la sesión es de otro usuario se responde igual que si no existiera
        if (sesion is null || sesion.UsuarioId != request.UsuarioId)
            throw new NotFoundException($"No existe una sesión con el id {request.SesionId}.");

        if (sesion.EstaRevocada)
            return;

        var motivo = request.UsuarioId == _usuarioActual.UsuarioId
            ? MotivoRevocacionSesion.CerradaPorUsuario
            : MotivoRevocacionSesion.CerradaPorAdministrador;

        sesion.Revocar(motivo, _dateTime.Now, _usuarioActual.UsuarioId);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
