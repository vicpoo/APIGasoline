using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Application.Common.Interfaces;
using APIGasoline.Domain.Interfaces;

namespace APIGasoline.Application.Common.Security;

/// <summary>
/// Techo de privilegios: nadie puede dar (ni quitar) más de lo que tiene.
/// - Un privilegio solo lo pone o lo quita de un rol quien tiene ese privilegio.
/// - Un rol solo lo asigna, lo quita, lo edita o lo borra quien tiene TODOS sus privilegios.
/// - A otro usuario solo lo desactiva, le cambia el correo o le cierra sesiones quien tiene TODOS
///   los privilegios de ese usuario (A008): así nadie deja fuera a alguien con más poder que él.
/// Así un administrador con rol.gestionar y rol.asignar no puede armarse un rol más poderoso que
/// el suyo ni quitarle a otro un rol que él no podría dar. La plataforma tiene todos los privilegios.
/// </summary>
public static class TechoPrivilegios
{
    public static async Task ExigirPrivilegioAsync(IUsuarioActual usuario, string clave, CancellationToken cancellationToken)
    {
        if (!await usuario.TienePrivilegioAsync(clave, cancellationToken))
            throw new ForbiddenException($"No puedes dar ni quitar el privilegio '{clave}' porque tú no lo tienes.");
    }

    public static async Task ExigirRolAsync(IUnitOfWork unitOfWork, IUsuarioActual usuario, Guid rolId, CancellationToken cancellationToken)
    {
        var delRol = await unitOfWork.Privilegios.GetClavesActivasByRolesAsync([rolId], cancellationToken);
        var mios = await usuario.GetPrivilegiosAsync(cancellationToken);

        var faltan = delRol.Where(clave => !mios.Contains(clave)).OrderBy(clave => clave, StringComparer.Ordinal).ToList();

        if (faltan.Count > 0)
            throw new ForbiddenException(
                $"El rol tiene privilegios que tú no tienes ({string.Join(", ", faltan)}): no puedes asignarlo, quitarlo, editarlo ni borrarlo.");
    }

    /// <summary>
    /// A008: para administrar la cuenta de otro usuario (estado, correo, sesiones) hay que tener todos sus
    /// privilegios efectivos. Sobre la propia cuenta no aplica. Un usuario inactivo no tiene privilegios
    /// efectivos, así que reactivarlo no exige nada extra.
    /// </summary>
    public static async Task ExigirUsuarioAsync(
        IUnitOfWork unitOfWork,
        IUsuarioActual usuario,
        Guid usuarioDestinoId,
        Guid? empresaDestinoId,
        CancellationToken cancellationToken)
    {
        if (usuarioDestinoId == usuario.UsuarioId)
            return;

        var delDestino = await unitOfWork.Privilegios.GetClavesEfectivasAsync(usuarioDestinoId, empresaDestinoId, cancellationToken);
        var mios = await usuario.GetPrivilegiosAsync(cancellationToken);

        var faltan = delDestino.Where(clave => !mios.Contains(clave)).Distinct().OrderBy(clave => clave, StringComparer.Ordinal).ToList();

        if (faltan.Count > 0)
            throw new ForbiddenException(
                $"Ese usuario tiene privilegios que tú no tienes ({string.Join(", ", faltan)}): no puedes desactivarlo, cambiar su correo ni cerrar sus sesiones.");
    }
}
