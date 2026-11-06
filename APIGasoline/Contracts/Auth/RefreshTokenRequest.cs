using System.ComponentModel.DataAnnotations;

namespace APIGasoline.Contracts.Auth;

/// <summary>Cuerpo de POST /api/auth/refresh y POST /api/auth/logout.</summary>
public class RefreshTokenRequest
{
    [Required]
    [StringLength(200)]
    public string RefreshToken { get; set; } = string.Empty;
}
