using APIGasoline.Application.Common.Constants;
using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Application.Common.Interfaces;

namespace APIGasoline.Application.Features.Auditoria.Common;

/// <summary>
/// El módulo de auditoría es secundario y solo lo usan perfiles autorizados:
/// Administrador, Consultor/Reportes, Confirmador y Gestor de activos.
/// Un empleado solo ve la información de su empresa (la del token).
/// </summary>
public static class PermisosAuditoria
{
    private static readonly string[] RolesAutorizados =
    [
        NombresRol.Administrador,
        NombresRol.ConsultorReportes,
        NombresRol.Confirmador,
        NombresRol.GestorActivos
    ];

    /// <summary>
    /// Valida el acceso y devuelve la empresa que se debe consultar.
    /// El Administrador puede consultar cualquier empresa (o todas, con null);
    /// los demás solo la suya, aunque manden otra.
    /// </summary>
    public static Guid? ResolverEmpresa(IUsuarioActual usuario, Guid? empresaSolicitada)
    {
        if (!usuario.EstaAutenticado)
            throw new UnauthorizedException("Debes iniciar sesión para consultar la auditoría.");

        if (!RolesAutorizados.Any(usuario.TieneRol))
            throw new ForbiddenException("Tu perfil no tiene acceso al módulo de auditoría y trazabilidad.");

        if (usuario.TieneRol(NombresRol.Administrador))
            return empresaSolicitada;

        if (usuario.EmpresaId is null)
            throw new ForbiddenException("Tu usuario no está vinculado a una empresa.");

        if (empresaSolicitada is not null && empresaSolicitada != usuario.EmpresaId)
            throw new ForbiddenException("No puedes consultar la auditoría de otra empresa.");

        return usuario.EmpresaId;
    }

    /// <summary>Valida que un registro pertenezca a la empresa que el usuario puede ver.</summary>
    public static void ValidarEmpresa(IUsuarioActual usuario, Guid empresaDelRegistro)
    {
        var empresa = ResolverEmpresa(usuario, null);

        if (empresa is not null && empresa != empresaDelRegistro)
            throw new ForbiddenException("El registro pertenece a otra empresa.");
    }
}
