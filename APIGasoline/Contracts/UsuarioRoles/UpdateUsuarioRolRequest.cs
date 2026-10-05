using System.ComponentModel.DataAnnotations;

namespace APIGasoline.Contracts.UsuarioRoles;

public class UpdateUsuarioRolRequest
{
    [Range(1, int.MaxValue)]
    public int UsuarioId { get; set; }

    [Range(1, int.MaxValue)]
    public int RolId { get; set; }
}
