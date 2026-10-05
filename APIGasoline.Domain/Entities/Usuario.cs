using APIGasoline.Domain.Enums;

namespace APIGasoline.Domain.Entities;

public class Usuario
{
    public int UsuarioId { get; set; }
    public string Email { get; set; } = string.Empty;
    public EstadoUsuario Estado { get; set; } = EstadoUsuario.Activo;
}