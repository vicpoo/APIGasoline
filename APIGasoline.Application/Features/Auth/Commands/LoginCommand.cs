using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Application.Common.Interfaces;
using APIGasoline.Application.Features.Auth.Common;
using APIGasoline.Application.Features.Auth.DTOs;
using APIGasoline.Domain.Interfaces;
using AutoMapper;
using MediatR;

namespace APIGasoline.Application.Features.Auth.Commands;

/// <summary>
/// Valida email + contraseña y devuelve el token JWT junto con los datos del usuario y su empleado.
/// </summary>
public record LoginCommand(string Email, string Password) : IRequest<LoginResponseDto>;

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

    public LoginCommandHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator,
        IFileStorageService storage)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
        _storage = storage;
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

        var token = _jwtTokenGenerator.Generar(new TokenUsuario(
            usuario.UsuarioId,
            usuario.Email,
            empleado?.EmpleadoId,
            empleado?.EmpresaId,
            perfil.Roles));

        return new LoginResponseDto(token.Token, "Bearer", token.ExpiraEnUtc, perfil);
    }
}
