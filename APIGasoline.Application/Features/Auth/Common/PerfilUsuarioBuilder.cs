using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Application.Features.Auth.DTOs;
using APIGasoline.Application.Features.Empleados.DTOs;
using APIGasoline.Application.Features.Usuarios.DTOs;
using APIGasoline.Domain.Entities;
using APIGasoline.Domain.Interfaces;
using AutoMapper;

namespace APIGasoline.Application.Features.Auth.Common;

/// <summary>
/// Arma el perfil del usuario (empleado, roles y privilegios) y valida que pueda entrar.
/// Se comparte entre el login y el endpoint de perfil.
/// </summary>
internal static class PerfilUsuarioBuilder
{
    public static async Task<(PerfilUsuarioDto Perfil, Empleado? Empleado)> ConstruirAsync(
        IUnitOfWork unitOfWork,
        IMapper mapper,
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

        var privilegios = await unitOfWork.Privilegios.GetNombresActivosByRolesAsync(
            roles.Select(r => r.RolId).ToList(), cancellationToken);

        var perfil = new PerfilUsuarioDto(
            mapper.Map<UsuarioDto>(usuario),
            empleado is null ? null : mapper.Map<EmpleadoDto>(empleado),
            roles.Select(r => r.Nombre).ToList(),
            privilegios);

        return (perfil, empleado);
    }
}
