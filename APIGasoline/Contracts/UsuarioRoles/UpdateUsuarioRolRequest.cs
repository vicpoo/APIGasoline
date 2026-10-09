using System.ComponentModel.DataAnnotations;

namespace APIGasoline.Contracts.UsuarioRoles;

public class UpdateUsuarioRolRequest
{
    [GuidNoVacio]
    public Guid UsuarioId { get; set; }

    [GuidNoVacio]
    public Guid RolId { get; set; }
}
