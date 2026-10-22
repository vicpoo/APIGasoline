using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Application.Common.Interfaces;

namespace APIGasoline.Application.Common.Security;

/// <summary>
/// Hasta dónde llegan los datos que puede ver o tocar el usuario, según sus privilegios:
/// - Empresa: la del token, salvo que tenga <see cref="Permisos.PlataformaMultiempresa"/>.
/// - Propios vs todos: sin el privilegio "ver todos", solo sus propios registros.
/// </summary>
public static class AlcanceUsuario
{
    /// <summary>
    /// Empresa que se debe consultar. Con plataforma.multiempresa devuelve la solicitada
    /// (null = todas); si no, la del token (y rechaza si pidió otra).
    /// </summary>
    public static async Task<Guid?> ResolverEmpresaAsync(
        this IUsuarioActual usuario,
        Guid? empresaSolicitada,
        CancellationToken cancellationToken = default)
    {
        if (!usuario.EstaAutenticado)
            throw new UnauthorizedException("Debes iniciar sesión.");

        if (await usuario.TienePrivilegioAsync(Permisos.PlataformaMultiempresa, cancellationToken))
            return empresaSolicitada;

        if (usuario.EmpresaId is null)
            throw new ForbiddenException("Tu usuario no está vinculado a una empresa.");

        if (empresaSolicitada is not null && empresaSolicitada != usuario.EmpresaId)
            throw new ForbiddenException("No puedes consultar información de otra empresa.");

        return usuario.EmpresaId;
    }

    /// <summary>Valida que un registro pertenezca a una empresa que el usuario puede ver.</summary>
    public static async Task ValidarEmpresaAsync(
        this IUsuarioActual usuario,
        Guid empresaDelRegistro,
        CancellationToken cancellationToken = default)
    {
        var empresa = await usuario.ResolverEmpresaAsync(null, cancellationToken);

        if (empresa is not null && empresa != empresaDelRegistro)
            throw new ForbiddenException("El registro pertenece a otra empresa.");
    }

    /// <summary>
    /// Empleado por el que se filtra una lista. Con <paramref name="privilegioVerTodos"/> devuelve el
    /// solicitado (null = todos); si no, fuerza el empleado del token (y rechaza si pidió otro).
    /// </summary>
    public static async Task<Guid?> ResolverEmpleadoAsync(
        this IUsuarioActual usuario,
        Guid? empleadoSolicitado,
        string privilegioVerTodos,
        CancellationToken cancellationToken = default)
    {
        if (await usuario.TienePrivilegioAsync(privilegioVerTodos, cancellationToken))
            return empleadoSolicitado;

        if (usuario.EmpleadoId is null)
            throw new ForbiddenException("Tu usuario no está vinculado a un empleado.");

        if (empleadoSolicitado is not null && empleadoSolicitado != usuario.EmpleadoId)
            throw new ForbiddenException("Solo puedes consultar tus propios registros.");

        return usuario.EmpleadoId;
    }

    /// <summary>
    /// Para endpoints "lo mío o, con privilegio, lo de cualquiera" (perfil, foto de licencia,
    /// contraseña, notificaciones...). Pasa si el registro es suyo o si tiene el privilegio.
    /// </summary>
    public static async Task ValidarPropioOPrivilegioAsync(
        this IUsuarioActual usuario,
        bool esPropio,
        string privilegio,
        CancellationToken cancellationToken = default)
    {
        if (!usuario.EstaAutenticado)
            throw new UnauthorizedException("Debes iniciar sesión.");

        if (esPropio)
            return;

        if (!await usuario.TienePrivilegioAsync(privilegio, cancellationToken))
            throw new ForbiddenException($"Solo puedes modificar tu propia información (o necesitas el privilegio '{privilegio}').");
    }
}
