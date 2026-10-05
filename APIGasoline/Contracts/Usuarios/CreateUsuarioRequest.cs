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
}
