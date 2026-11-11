using APIGasoline.Application.Common.Interfaces;
using APIGasoline.Application.Common.Sesiones;
using APIGasoline.Domain.Enums;
using APIGasoline.Domain.Interfaces;
using APIGasoline.Application.Common.Messaging;

namespace APIGasoline.Application.Features.Auth.Commands;

/// <summary>
/// Logout: cierra la sesión del refresh token indicado. Funciona aunque el JWT de acceso ya haya
/// expirado (el refresh token es la prueba de que la sesión es suya).
/// Es idempotente y nunca revela si el token existía: siempre termina sin error.
/// </summary>
public sealed record CerrarSesionCommand(string RefreshToken) : ICommand;

public sealed class CerrarSesionCommandHandler : ICommandHandler<CerrarSesionCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _dateTime;

    public CerrarSesionCommandHandler(IUnitOfWork unitOfWork, IDateTimeProvider dateTime)
    {
        _unitOfWork = unitOfWork;
        _dateTime = dateTime;
    }

    public async Task Handle(CerrarSesionCommand request, CancellationToken cancellationToken)
    {
        if (!TokenRefresh.FormatoValido(request.RefreshToken))
            return;

        var hash = TokenRefresh.Hash(request.RefreshToken.Trim());
        var sesion = await _unitOfWork.Sesiones.GetByTokenHashAsync(hash, cancellationToken);

        if (sesion is null || sesion.EstaRevocada)
            return;

        // Un token ya reemplazado también cierra la sesión: quien lo manda quiere salir
        sesion.Revocar(MotivoRevocacionSesion.CierreSesion, _dateTime.Now);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
