using APIGasoline.Domain.Enums;
using APIGasoline.Domain.Exceptions;

namespace APIGasoline.Domain.Entities;

public class Usuario
{
    public const int EmailMaxLength = 150;
    public const int PasswordHashMaxLength = 255;

    public Guid UsuarioId { get; private set; } = Guid.CreateVersion7();
    public string Email { get; private set; } = string.Empty;

 
    public string PasswordHash { get; private set; } = string.Empty;

    public EstadoUsuario Estado { get; private set; } = EstadoUsuario.Activo;

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
