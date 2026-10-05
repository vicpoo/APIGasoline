using APIGasoline.Domain.Exceptions;

namespace APIGasoline.Domain.Entities;

/// <summary>
/// Relación N:M entre roles y privilegios. Un rol puede tener varios privilegios
/// y un privilegio puede pertenecer a varios roles.
/// </summary>
public class RolPrivilegio
{
    public int RolPrivilegioId { get; private set; }
    public int RolId { get; private set; }
    public int PrivilegioId { get; private set; }

    // Requerido por Entity Framework
    private RolPrivilegio()
    {
    }

    public static RolPrivilegio Crear(int rolId, int privilegioId)
    {
        var rolPrivilegio = new RolPrivilegio();
        rolPrivilegio.Reasignar(rolId, privilegioId);

        return rolPrivilegio;
    }

    public void Reasignar(int rolId, int privilegioId)
    {
        if (rolId <= 0)
            throw new DomainException("El id del rol debe ser mayor a cero.");

        if (privilegioId <= 0)
            throw new DomainException("El id del privilegio debe ser mayor a cero.");

        RolId = rolId;
        PrivilegioId = privilegioId;
    }
}
