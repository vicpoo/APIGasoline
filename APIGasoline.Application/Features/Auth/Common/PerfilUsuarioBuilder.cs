using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Application.Common.Interfaces;
using APIGasoline.Application.Common.Storage;
using APIGasoline.Application.Features.Auth.DTOs;
using APIGasoline.Application.Features.Empleados.DTOs;
using APIGasoline.Application.Features.Usuarios.DTOs;
using APIGasoline.Domain.Entities;
using APIGasoline.Domain.Interfaces;
using AutoMapper;
using CalculoVersion = APIGasoline.Application.Common.Security.VersionPrivilegios;

namespace APIGasoline.Application.Features.Auth.Common;

/// <summary>Empleado, roles activos y claves de privilegios efectivos de un usuario.</summary>
internal record AccesoUsuario(Empleado? Empleado, IReadOnlyList<Rol> Roles, IReadOnlyList<string> Privilegios)
{
    public string VersionPrivilegios => CalculoVersion.Calcular(Privilegios);
}

/// <summary>
/// Arma el perfil del usuario (empleado, roles y privilegios) y valida que pueda entrar.
/// Se comparte entre el login, el endpoint de perfil y el de privilegios.
/// </summary>
internal static class PerfilUsuarioBuilder
{
    /// <summary>
    /// Valida usuario/empleado/empresa y calcula los privilegios efectivos:
    /// unión de los privilegios ACTIVOS de los roles ACTIVOS del usuario que sean globales
    /// o de su propia empresa (un rol de otra empresa nunca da permisos, aunque esté asignado).
    /// </summary>
    public static async Task<AccesoUsuario> CargarAccesoAsync(
        IUnitOfWork unitOfWork,
        Usuario usuario,
        CancellationToken cancellationToken)
    {
        if (!usuario.EstaActivo)
            throw new ForbiddenException("El usuario está inactivo.");

        // Un usuario tiene como máximo un empleado (1:1). Puede no tener (ej. administrador interno).
        var empleado = await unitOfWork.Empleados.GetByUsuarioIdAsync(usuario.UsuarioId, cancellationToken);

        if (empleado is null && await unitOfWork.Empleados.UsuarioAsignadoAsync(usuario.UsuarioId, null, cancellationToken))
            throw new ForbiddenException("El empleado ligado a este usuario fue dado de baja.");

        if (empleado is not null && !await unitOfWork.Empresas.ExistsAsync(empleado.EmpresaId, cancellationToken))
            throw new ForbiddenException("La empresa del empleado fue dada de baja.");

        var roles = await unitOfWork.Roles.GetActivosByUsuarioAsync(usuario.UsuarioId, empleado?.EmpresaId, cancellationToken);

        var privilegios = await unitOfWork.Privilegios.GetClavesActivasByRolesAsync(
            roles.Select(r => r.RolId).ToList(), cancellationToken);

        return new AccesoUsuario(empleado, roles, privilegios);
    }

    public static async Task<(PerfilUsuarioDto Perfil, Empleado? Empleado)> ConstruirAsync(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IFileStorageService storage,
        Usuario usuario,
        CancellationToken cancellationToken)
    {
        var acceso = await CargarAccesoAsync(unitOfWork, usuario, cancellationToken);

        var empleadoDto = acceso.Empleado is null
            ? null
            : await mapper.Map<EmpleadoDto>(acceso.Empleado).ConUrlsAsync(storage, cancellationToken);

        var perfil = new PerfilUsuarioDto(
            mapper.Map<UsuarioDto>(usuario),
            empleadoDto,
            acceso.Roles.Select(r => r.Nombre).ToList(),
            acceso.Privilegios,
            acceso.VersionPrivilegios);

        return (perfil, acceso.Empleado);
    }
}
