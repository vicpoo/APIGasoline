using System.ComponentModel.DataAnnotations;

namespace APIGasoline.Contracts.Empleados;

public class CreateEmpleadoRequest
{
    [GuidNoVacio]
    public Guid EmpresaId { get; set; }

    [GuidNoVacio]
    public Guid UsuarioId { get; set; }

    [Required]
    [StringLength(150)]
    public string NombreCompleto { get; set; } = string.Empty;
}
