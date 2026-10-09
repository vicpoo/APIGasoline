using APIGasoline.Domain.Exceptions;

namespace APIGasoline.Domain.Entities;


public class UsuarioRol
{
    public Guid UsuarioRolId { get; private set; } = Guid.CreateVersion7();
    public Guid UsuarioId { get; private set; }
    public Guid RolId { get; private set; }

    // Requerido por Entity Framework
    private UsuarioRol()
    {
    }

    public static UsuarioRol Crear(Guid usuarioId, Guid rolId)
    {
        var usuarioRol = new UsuarioRol();
        usuarioRol.Reasignar(usuarioId, rolId);

        return usuarioRol;
    }

    public void Reasignar(Guid usuarioId, Guid rolId)
    {
        if (usuarioId == Guid.Empty)
            throw new DomainException("El id del usuario es obligatorio.");

        if (rolId == Guid.Empty)
            throw new DomainException("El id del rol es obligatorio.");

        UsuarioId = usuarioId;
        RolId = rolId;
    }
}
