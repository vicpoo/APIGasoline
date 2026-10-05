using System.ComponentModel.DataAnnotations;
using APIGasoline.Domain.Enums;

namespace APIGasoline.Contracts.Usuarios;

public class UpdateUsuarioRequest
{
    [Required]
    [EmailAddress]
    [StringLength(150)]
    public string Email { get; set; } = string.Empty;

    public EstadoUsuario Estado { get; set; } = EstadoUsuario.Activo;
}