using System.ComponentModel.DataAnnotations;

namespace APIGasoline.Contracts.RolPrivilegios;

public class CreateRolPrivilegioRequest
{
    [GuidNoVacio]
    public Guid RolId { get; set; }

    [GuidNoVacio]
    public Guid PrivilegioId { get; set; }
}
