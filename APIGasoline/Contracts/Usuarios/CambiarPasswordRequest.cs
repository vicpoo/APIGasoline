using System.ComponentModel.DataAnnotations;

namespace APIGasoline.Contracts.Usuarios;

public class CambiarPasswordRequest
{
    [Required]
    public string PasswordActual { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 8)]
    public string PasswordNueva { get; set; } = string.Empty;
}
