using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Application.Common.Interfaces;
using APIGasoline.Application.Common.Sesiones;
using APIGasoline.Application.Features.Auth.Common;
using APIGasoline.Application.Features.Auth.DTOs;
using APIGasoline.Domain.Entities;
using APIGasoline.Domain.Enums;
using APIGasoline.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace APIGasoline.Application.Features.Auth.Commands;

/// <summary>
/// Renueva la sesión con el refresh token: devuelve un JWT de acceso nuevo y un refresh token nuevo.
///
/// Reglas de seguridad:
/// - Rotación: el refresh token usado deja de servir (el nuevo lo reemplaza).
/// - Reuso: si llega un token que YA fue reemplazado, alguien más lo tiene (robo) y se cierra la sesión
///   completa. Excepción: si la rotación fue hace menos de Sesiones:GraciaReusoSegundos, se asume que el
///   mismo celular mandó dos renovaciones al mismo tiempo; se rechaza sin cerrar la sesión.
/// - Se vuelve a validar que el usuario, su empleado y su empresa sigan activos.
/// Cualquier rechazo responde 401: la app debe borrar la sesión local y pedir login.
/// </summary>
public record RefrescarSesionCommand(string RefreshToken, DatosClienteSesion? Cliente = null) : IRequest<SesionTokensDto>;

public class RefrescarSesionCommandHandler : IRequestHandler<RefrescarSesionCommand, SesionTokensDto>
{
    private const string SesionInvalida = "La sesión no es válida o ya expiró. Inicia sesión de nuevo.";
    private const string SesionYaRenovada = "La sesión ya se renovó con otra petición. Usa el token más reciente.";

    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IDateTimeProvider _dateTime;
    private readonly SesionesSettings _sesiones;
    private readonly ILogger<RefrescarSesionCommandHandler> _logger;

    public RefrescarSesionCommandHandler(
        IUnitOfWork unitOfWork,
        IJwtTokenGenerator jwtTokenGenerator,
        IDateTimeProvider dateTime,
        SesionesSettings sesiones,
        ILogger<RefrescarSesionCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _jwtTokenGenerator = jwtTokenGenerator;
        _dateTime = dateTime;
        _sesiones = sesiones;
        _logger = logger;
    }

    public async Task<SesionTokensDto> Handle(RefrescarSesionCommand request, CancellationToken cancellationToken)
    {
        if (!TokenRefresh.FormatoValido(request.RefreshToken))
            throw new UnauthorizedException(SesionInvalida);

        var ahora = _dateTime.Now;
        var hash = TokenRefresh.Hash(request.RefreshToken.Trim());

        var sesion = await _unitOfWork.Sesiones.GetByTokenHashAsync(hash, cancellationToken)
            ?? throw new UnauthorizedException(SesionInvalida);

        if (sesion.EsTokenAnterior(hash))
            await RechazarReusoAsync(sesion, ahora, cancellationToken);

        if (!sesion.EstaActiva(ahora))
            throw new UnauthorizedException(SesionInvalida);

        var usuario = await _unitOfWork.Usuarios.GetByIdAsync(sesion.UsuarioId, cancellationToken);

        if (usuario is null)
            await CerrarYRechazarAsync(sesion, MotivoRevocacionSesion.AccesoRevocado, ahora, SesionInvalida, cancellationToken);

        // Mismas reglas que el login: usuario activo, empleado y empresa vigentes; roles actualizados
        AccesoUsuario acceso;

        try
        {
            acceso = await PerfilUsuarioBuilder.CargarAccesoAsync(_unitOfWork, usuario!, cancellationToken);
        }
        catch (ForbiddenException ex)
        {
            await CerrarYRechazarAsync(sesion, MotivoRevocacionSesion.AccesoRevocado, ahora, ex.Message, cancellationToken);
            throw; // inalcanzable: CerrarYRechazarAsync siempre lanza
        }

        var cliente = request.Cliente ?? DatosClienteSesion.Vacio;
        var nuevoRefreshToken = TokenRefresh.Generar();

        sesion.Rotar(TokenRefresh.Hash(nuevoRefreshToken), ahora, _sesiones.DuracionRefresh, cliente.Ip, cliente.UserAgent);

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex) when (EsConflictoDeConcurrencia(ex))
        {
            // Otra petición con el MISMO token ganó la rotación un instante antes (token_hash es concurrency token)
            throw new UnauthorizedException(SesionYaRenovada);
        }

        var roles = acceso.Roles.Select(r => r.Nombre).ToList();
        var token = EmisionTokens.GenerarAcceso(_jwtTokenGenerator, usuario!, acceso.Empleado, roles, sesion.SesionUsuarioId);

        return new SesionTokensDto(
            token.Token,
            "Bearer",
            token.ExpiraEnUtc,
            nuevoRefreshToken,
            EmisionTokens.AUtc(sesion.ExpiraEn),
            sesion.SesionUsuarioId,
            acceso.VersionPrivilegios);
    }

    /// <summary>
    /// Llegó un refresh token que ya había sido reemplazado. Dentro del periodo de gracia es una
    /// carrera del mismo celular; fuera de él es señal de robo y se cierra la sesión completa.
    /// Siempre lanza 401.
    /// </summary>
    private async Task RechazarReusoAsync(SesionUsuario sesion, DateTime ahora, CancellationToken cancellationToken)
    {
        if (sesion.EstaRevocada || sesion.RotadaRecientemente(ahora, _sesiones.GraciaReuso))
            throw new UnauthorizedException(sesion.EstaRevocada ? SesionInvalida : SesionYaRenovada);

        _logger.LogWarning(
            "Reuso de refresh token detectado en la sesión {SesionId} del usuario {UsuarioId}: se cierra la sesión.",
            sesion.SesionUsuarioId, sesion.UsuarioId);

        await CerrarYRechazarAsync(sesion, MotivoRevocacionSesion.ReusoDetectado, ahora, SesionInvalida, cancellationToken);
    }

    private async Task CerrarYRechazarAsync(
        SesionUsuario sesion,
        MotivoRevocacionSesion motivo,
        DateTime ahora,
        string mensaje,
        CancellationToken cancellationToken)
    {
        sesion.Revocar(motivo, ahora);

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex) when (EsConflictoDeConcurrencia(ex))
        {
            // Otra petición la modificó al mismo tiempo; el rechazo se mantiene
        }

        throw new UnauthorizedException(mensaje);
    }

    /// <summary>DbUpdateConcurrencyException de EF (Application no depende de EF, por eso se compara el nombre).</summary>
    private static bool EsConflictoDeConcurrencia(Exception ex)
        => ex.GetType().Name == "DbUpdateConcurrencyException";
}
