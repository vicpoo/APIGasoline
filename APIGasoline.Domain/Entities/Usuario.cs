using APIGasoline.Domain.Enums;
using APIGasoline.Domain.Exceptions;

namespace APIGasoline.Domain.Entities;

/// <summary>
/// Credenciales de acceso. El login se valida contra esta tabla (email + password_hash).
/// Un usuario pertenece a una sola empresa a través de su registro en empleado.
/// </summary>
public class Usuario
{
    public const int EmailMaxLength = 150;
    public const int PasswordHashMaxLength = 255;

    public int UsuarioId { get; private set; }
    public string Email { get; private set; } = string.Empty;

    /// <summary>
    /// Hash de la contraseña (nunca la contraseña en texto plano).
    /// El hash lo calcula Infrastructure (IPasswordHasher); el dominio solo lo guarda.
    /// </summary>
    public string PasswordHash { get; private set; } = string.Empty;

    public EstadoUsuario Estado { get; private set; } = EstadoUsuario.Activo;

    /// <summary>La asigna la base de datos al insertar (default now()).</summary>
    public DateTime? FechaRegistro { get; private set; }

    // Requerido por Entity Framework
    private Usuario()
    {
    }

    public static Usuario Crear(string email, string passwordHash, EstadoUsuario estado = EstadoUsuario.Activo)
    {
        var usuario = new Usuario();
        usuario.CambiarEmail(email);
        usuario.CambiarPasswordHash(passwordHash);
        usuario.CambiarEstado(estado);

        return usuario;
    }

    public bool EstaActivo => Estado == EstadoUsuario.Activo;

    public void CambiarEmail(string email)
    {
        // Se guarda en minúsculas para que el login no dependa de mayúsculas
        var normalizado = email?.Trim().ToLowerInvariant() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(normalizado))
            throw new DomainException("El email es obligatorio.");

        if (normalizado.Length > EmailMaxLength)
            throw new DomainException($"El email no puede exceder {EmailMaxLength} caracteres.");

        Email = normalizado;
    }

    public void CambiarPasswordHash(string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new DomainException("El hash de la contraseña es obligatorio.");

        if (passwordHash.Length > PasswordHashMaxLength)
            throw new DomainException($"El hash de la contraseña no puede exceder {PasswordHashMaxLength} caracteres.");

        PasswordHash = passwordHash;
    }

    public void CambiarEstado(EstadoUsuario estado)
    {
        if (!Enum.IsDefined(estado))
            throw new DomainException("El estado del usuario no es válido.");

        Estado = estado;
    }

    public void Activar() => Estado = EstadoUsuario.Activo;

    public void Desactivar() => Estado = EstadoUsuario.Inactivo;
}
