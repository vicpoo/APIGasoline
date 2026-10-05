using System.ComponentModel.DataAnnotations;

namespace APIGasoline.Contracts.Empleados;

public class UpdateEmpleadoRequest
{
    [Range(1, int.MaxValue)]
    public int EmpresaId { get; set; }

    [Range(1, int.MaxValue)]
    public int UsuarioId { get; set; }

    [Required]
    [StringLength(150)]
    public string NombreCompleto { get; set; } = string.Empty;

    [StringLength(255)]
    public string? FotoPerfil { get; set; }

    [StringLength(255)]
    public string? FotoLicencia { get; set; }
}
