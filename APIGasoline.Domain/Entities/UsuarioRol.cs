using APIGasoline.Domain.Exceptions;

namespace APIGasoline.Domain.Entities;


public class UsuarioRol
{
    public int UsuarioRolId { get; private set; }
    public int UsuarioId { get; private set; }
    public int RolId { get; private set; }

    // Requerido por Entity Framework
    private UsuarioRol()
    {
    }

    public static UsuarioRol Crear(int usuarioId, int rolId)
    {
        var usuarioRol = new UsuarioRol();
        usuarioRol.Reasignar(usuarioId, rolId);

        return usuarioRol;
    }

    public void Reasignar(int usuarioId, int rolId)
    {
        if (usuarioId <= 0)
            throw new DomainException("El id del usuario debe ser mayor a cero.");

        if (rolId <= 0)
            throw new DomainException("El id del rol debe ser mayor a cero.");

        UsuarioId = usuarioId;
        RolId = rolId;
    }
}
