using System.ComponentModel.DataAnnotations;
using APIGasoline.Domain.Enums;

namespace APIGasoline.Contracts.Usuarios;

public class CreateUsuarioRequest
{
    [Range(1, int.MaxValue)]
    public int UsuarioId { get; set; }

    [Required]
    [EmailAddress]
    [StringLength(150)]
    public string Email { get; set; } = string.Empty;

    public EstadoUsuario Estado { get; set; } = EstadoUsuario.Activo;
}