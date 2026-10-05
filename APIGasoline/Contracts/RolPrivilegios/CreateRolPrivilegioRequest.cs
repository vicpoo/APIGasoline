using System.ComponentModel.DataAnnotations;

namespace APIGasoline.Contracts.RolPrivilegios;

public class CreateRolPrivilegioRequest
{
    [Range(1, int.MaxValue)]
    public int RolId { get; set; }

    [Range(1, int.MaxValue)]
    public int PrivilegioId { get; set; }
}
