using System.Security.Claims;
using System.Text;
using APIGasoline.Application.Common.Interfaces;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace APIGasoline.Infrastructure.Authentication;

public sealed class JwtTokenGenerator : IJwtTokenGenerator
{
    public const string ClaimEmpleadoId = "empleado_id";
    public const string ClaimEmpresaId = "empresa_id";
    public const string ClaimRol = "role";

    private readonly JwtSettings _settings;
    private readonly IDateTimeProvider _dateTime;

    public JwtTokenGenerator(JwtSettings settings, IDateTimeProvider dateTime)
    {
        _settings = settings;
        _dateTime = dateTime;
    }

    public TokenGenerado Generar(TokenUsuario usuario)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, usuario.UsuarioId.ToString()),
            new(JwtRegisteredClaimNames.Email, usuario.Email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        if (usuario.EmpleadoId is not null)
            claims.Add(new Claim(ClaimEmpleadoId, usuario.EmpleadoId.Value.ToString()));

        if (usuario.EmpresaId is not null)
            claims.Add(new Claim(ClaimEmpresaId, usuario.EmpresaId.Value.ToString()));

        claims.AddRange(usuario.Roles.Select(rol => new Claim(ClaimRol, rol)));

        var expira = _dateTime.Now.ToUniversalTime().AddMinutes(_settings.ExpiracionMinutos);

        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Issuer = _settings.Issuer,
            Audience = _settings.Audience,
            Expires = expira,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Key)),
                SecurityAlgorithms.HmacSha256)
        };

        var token = new JsonWebTokenHandler().CreateToken(descriptor);

        return new TokenGenerado(token, expira);
    }
}
