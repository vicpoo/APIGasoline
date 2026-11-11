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

    /// <summary>
    /// Empresa a la que pertenece la cuenta. Null: usuario de la plataforma, o cuenta que todavía
    /// no se asigna (se le asigna al crearle su empleado). Una empresa solo ve a sus propios usuarios.
    /// </summary>
    public Guid? EmpresaId { get; private set; }

    // Requerido por Entity Framework
    private Usuario()
    {
    }

    public static Usuario Crear(string email, string passwordHash, EstadoUsuario estado = EstadoUsuario.Activo, Guid? empresaId = null)
    {
        if (empresaId == Guid.Empty)
            throw new DomainException("El id de la empresa no es válido.");

        var usuario = new Usuario { EmpresaId = empresaId };
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

    /// <summary>
    /// Asigna la cuenta a una empresa. Solo se puede si todavía no tiene una:
    /// una cuenta nunca se pasa de una empresa a otra.
    /// </summary>
    public void AsignarEmpresa(Guid empresaId)
    {
        if (empresaId == Guid.Empty)
            throw new DomainException("El id de la empresa es obligatorio.");

        if (EmpresaId is not null && EmpresaId != empresaId)
            throw new DomainException("El usuario ya pertenece a otra empresa.");

        EmpresaId = empresaId;
    }

    public bool PerteneceA(Guid empresaId) => EmpresaId == empresaId;

    public void Activar() => Estado = EstadoUsuario.Activo;

    public void Desactivar() => Estado = EstadoUsuario.Inactivo;
}
