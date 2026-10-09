using APIGasoline.Domain.Exceptions;

namespace APIGasoline.Domain.Entities;


public class RolPrivilegio
{
    public Guid RolPrivilegioId { get; private set; } = Guid.CreateVersion7();
    public Guid RolId { get; private set; }
    public Guid PrivilegioId { get; private set; }

    // Requerido por Entity Framework
    private RolPrivilegio()
    {
    }

    public static RolPrivilegio Crear(Guid rolId, Guid privilegioId)
    {
        var rolPrivilegio = new RolPrivilegio();
        rolPrivilegio.Reasignar(rolId, privilegioId);

        return rolPrivilegio;
    }

    public void Reasignar(Guid rolId, Guid privilegioId)
    {
        if (rolId == Guid.Empty)
            throw new DomainException("El id del rol es obligatorio.");

        if (privilegioId == Guid.Empty)
            throw new DomainException("El id del privilegio es obligatorio.");

        RolId = rolId;
        PrivilegioId = privilegioId;
    }
}
