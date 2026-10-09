using System.ComponentModel.DataAnnotations;

namespace APIGasoline.Contracts.UsuarioRoles;

public class CreateUsuarioRolRequest
{
    [GuidNoVacio]
    public Guid UsuarioId { get; set; }

    [GuidNoVacio]
    public Guid RolId { get; set; }
}
