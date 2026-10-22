using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Application.Common.Interfaces;
using APIGasoline.Application.Common.Security;

namespace APIGasoline.Application.Features.Auditoria.Common;

/// <summary>
/// El módulo de auditoría lo usa quien tenga el privilegio <see cref="Permisos.AuditoriaVer"/>
/// (sin importar cómo se llame su rol). Un empleado solo ve la información de su empresa
/// (la del token), salvo que tenga <see cref="Permisos.PlataformaMultiempresa"/>.
/// </summary>
public static class PermisosAuditoria
{
    /// <summary>
    /// Valida el acceso y devuelve la empresa que se debe consultar.
    /// Con plataforma.multiempresa puede consultar cualquier empresa (o todas, con null);
    /// los demás solo la suya, aunque manden otra.
    /// </summary>
    public static async Task<Guid?> ResolverEmpresaAsync(
        IUsuarioActual usuario,
        Guid? empresaSolicitada,
        CancellationToken cancellationToken = default)
    {
        if (!usuario.EstaAutenticado)
            throw new UnauthorizedException("Debes iniciar sesión para consultar la auditoría.");

        if (!await usuario.TienePrivilegioAsync(Permisos.AuditoriaVer, cancellationToken))
            throw new ForbiddenException("Tu perfil no tiene acceso al módulo de auditoría y trazabilidad.");

        return await usuario.ResolverEmpresaAsync(empresaSolicitada, cancellationToken);
    }

    /// <summary>Valida que un registro pertenezca a la empresa que el usuario puede ver.</summary>
    public static async Task ValidarEmpresaAsync(
        IUsuarioActual usuario,
        Guid empresaDelRegistro,
        CancellationToken cancellationToken = default)
    {
        var empresa = await ResolverEmpresaAsync(usuario, null, cancellationToken);

        if (empresa is not null && empresa != empresaDelRegistro)
            throw new ForbiddenException("El registro pertenece a otra empresa.");
    }
}
