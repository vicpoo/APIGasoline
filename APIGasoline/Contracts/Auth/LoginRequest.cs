using System.ComponentModel.DataAnnotations;
using APIGasoline.Domain.Enums;

namespace APIGasoline.Contracts.Auth;

public class LoginRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;

    /// <summary>Opcional: android | ios | web. Solo para mostrar la sesión en la lista de sesiones abiertas.</summary>
    public PlataformaDispositivo? Plataforma { get; set; }

    /// <summary>Opcional: nombre del dispositivo (ej. "Moto G84"). Solo informativo.</summary>
    [StringLength(100)]
    public string? NombreDispositivo { get; set; }
}
