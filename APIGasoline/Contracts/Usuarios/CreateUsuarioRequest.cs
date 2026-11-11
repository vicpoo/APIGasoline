using System.ComponentModel.DataAnnotations;
using APIGasoline.Domain.Enums;

namespace APIGasoline.Contracts.Usuarios;

public class CreateUsuarioRequest
{
    [Required]
    [EmailAddress]
    [StringLength(150)]
    public string Email { get; set; } = string.Empty;

    /// <summary>Contraseña en texto plano; la API solo guarda su hash.</summary>
    [Required]
    [StringLength(100, MinimumLength = 8)]
    public string Password { get; set; } = string.Empty;

    public EstadoUsuario Estado { get; set; } = EstadoUsuario.Activo;

    /// <summary>
    /// Empresa de la cuenta. Un administrador de empresa puede omitirla (se usa la suya) y no puede
    /// poner otra. La plataforma puede omitirla para crear una cuenta sin empresa (se le asigna al
    /// crearle su empleado).
    /// </summary>
    public Guid? EmpresaId { get; set; }
}
