using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Application.Common.Interfaces;
using APIGasoline.Application.Common.Sesiones;
using APIGasoline.Application.Features.Auth.Common;
using APIGasoline.Application.Features.Auth.DTOs;
using APIGasoline.Domain.Entities;
using APIGasoline.Domain.Enums;
using APIGasoline.Domain.Interfaces;
using AutoMapper;
using MediatR;

namespace APIGasoline.Application.Features.Auth.Commands;

/// <summary>
/// Valida email + contraseña, abre una sesión (refresh token) para el dispositivo y devuelve
/// el JWT de acceso, el refresh token y los datos del usuario y su empleado.
/// </summary>
public record LoginCommand(string Email, string Password, DatosClienteSesion? Cliente = null) : IRequest<LoginResponseDto>;

public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResponseDto>
{
    private const string CredencialesInvalidas = "Email o contraseña incorrectos.";

    // Hash de relleno: si el email no existe se verifica igual, para no revelar
    // por el tiempo de respuesta qué emails están registrados.
    private static string? _hashDeRelleno;

    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IFileStorageService _storage;
    private readonly IDateTimeProvider _dateTime;
    private readonly SesionesSettings _sesiones;

    public LoginCommandHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator,
        IFileStorageService storage,
        IDateTimeProvider dateTime,
        SesionesSettings sesiones)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
        _storage = storage;
        _dateTime = dateTime;
        _sesiones = sesiones;
    }

    public async Task<LoginResponseDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var email = request.Email?.Trim().ToLowerInvariant() ?? string.Empty;
        var usuario = await _unitOfWork.Usuarios.GetByEmailAsync(email, cancellationToken);

        if (usuario is null)
        {
            _hashDeRelleno ??= _passwordHasher.Hash("relleno-para-tiempo-constante-1");
            _passwordHasher.Verify(request.Password ?? string.Empty, _hashDeRelleno);
            throw new UnauthorizedException(CredencialesInvalidas);
        }

        if (!_passwordHasher.Verify(request.Password ?? string.Empty, usuario.PasswordHash))
            throw new UnauthorizedException(CredencialesInvalidas);

        // Valida estado del usuario/empleado/empresa y carga roles y privilegios
        var (perfil, empleado) = await PerfilUsuarioBuilder.ConstruirAsync(_unitOfWork, _mapper, _storage, usuario, cancellationToken);

        // Sesión del dispositivo: el refresh token se entrega UNA vez y en la BD solo queda su hash
        var ahora = _dateTime.Now;
        var cliente = request.Cliente ?? DatosClienteSesion.Vacio;
        var refreshToken = TokenRefresh.Generar();

        var sesion = SesionUsuario.Iniciar(
            usuario.UsuarioId,
            empleado?.EmpresaId,
            TokenRefresh.Hash(refreshToken),
            cliente.Plataforma,
            cliente.NombreDispositivo,
            cliente.Ip,
            cliente.UserAgent,
            ahora,
            _sesiones.DuracionRefresh,
            _sesiones.DuracionMaxima);

        await CerrarExcedentesAsync(usuario.UsuarioId, ahora, cancellationToken);
        await _unitOfWork.Sesiones.AddAsync(sesion, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var token = EmisionTokens.GenerarAcceso(_jwtTokenGenerator, usuario, empleado, perfil.Roles, sesion.SesionUsuarioId);

        return new LoginResponseDto(
            token.Token,
            "Bearer",
            token.ExpiraEnUtc,
            refreshToken,
            EmisionTokens.AUtc(sesion.ExpiraEn),
            sesion.SesionUsuarioId,
            perfil);
    }

    /// <summary>
    /// Deja lugar para la sesión nueva: si el usuario ya tiene el máximo de sesiones abiertas,
    /// cierra las que llevan más tiempo sin usarse.
    /// </summary>
    private async Task CerrarExcedentesAsync(Guid usuarioId, DateTime ahora, CancellationToken cancellationToken)
    {
        var activas = await _unitOfWork.Sesiones.GetActivasByUsuarioAsync(usuarioId, ahora, cancellationToken);
        var sobran = activas.Count - (_sesiones.MaxSesionesPorUsuario - 1);

        if (sobran <= 0)
            return;

        foreach (var vieja in activas.OrderBy(s => s.FechaUltimoUso).Take(sobran))
            vieja.Revocar(MotivoRevocacionSesion.LimiteSesiones, ahora);
    }
}
