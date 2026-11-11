using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Application.Common.Interfaces;
using APIGasoline.Application.Common.Security;
using APIGasoline.Application.Common.Sesiones;
using APIGasoline.Domain.Enums;
using APIGasoline.Domain.Interfaces;
using APIGasoline.Application.Common.Messaging;

namespace APIGasoline.Application.Features.Usuarios.Commands;

/// <summary>
/// Cambia la contraseña verificando primero la actual.
/// Cierra las sesiones abiertas del usuario en los demás dispositivos (si la cambia él mismo,
/// conserva la sesión desde la que lo hizo; si la cambia un administrador, las cierra todas).
/// </summary>
public sealed record CambiarPasswordCommand(Guid UsuarioId, string PasswordActual, string PasswordNueva) : ICommand, IRecursoPropio
{
    /// <summary>Su propia contraseña, o la de cualquiera con usuario.gestionar (igual se pide la actual).</summary>
    public bool EsPropio(IUsuarioActual usuario)
        => UsuarioId == usuario.UsuarioId;

    public string PrivilegioParaOtros => Permisos.UsuarioGestionar;
}

public sealed class CambiarPasswordCommandHandler : ICommandHandler<CambiarPasswordCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUsuarioActual _usuarioActual;
    private readonly IDateTimeProvider _dateTime;

    public CambiarPasswordCommandHandler(
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher,
        IUsuarioActual usuarioActual,
        IDateTimeProvider dateTime)
    {
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _usuarioActual = usuarioActual;
        _dateTime = dateTime;
    }

    public async Task Handle(CambiarPasswordCommand request, CancellationToken cancellationToken)
    {
        var usuario = await _unitOfWork.Usuarios.GetByIdAsync(request.UsuarioId, cancellationToken)
            ?? throw new NotFoundException($"No existe un usuario con el id {request.UsuarioId}.");

        if (!_passwordHasher.Verify(request.PasswordActual, usuario.PasswordHash))
            throw new UnauthorizedException("La contraseña actual es incorrecta.");

        if (request.PasswordActual == request.PasswordNueva)
            throw new BadRequestException("La contraseña nueva debe ser diferente a la actual.");

        PasswordPolicy.Validar(request.PasswordNueva);

        usuario.CambiarPasswordHash(_passwordHasher.Hash(request.PasswordNueva));

        var esPropio = usuario.UsuarioId == _usuarioActual.UsuarioId;

        await _unitOfWork.RevocarDeUsuarioAsync(
            usuario.UsuarioId,
            MotivoRevocacionSesion.CambioPassword,
            _dateTime.Now,
            _usuarioActual.UsuarioId,
            exceptoSesionId: esPropio ? _usuarioActual.SesionId : null,
            cancellationToken: cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
