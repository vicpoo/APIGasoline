using APIGasoline.Application.Common.Constants;
using APIGasoline.Application.Common.Interfaces;
using APIGasoline.Application.Common.Security;
using APIGasoline.Domain.Entities;
using APIGasoline.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace APIGasoline.Application.Features.Auth.Commands;

/// <summary>
/// Siembra idempotente de la seguridad (la API la manda al arrancar; se puede repetir sin problema):
/// 1. Crea los privilegios de <see cref="Permisos.Catalogo"/> que falten (no toca los existentes).
/// 2. Crea el rol global "Administrador" si no existe y le asigna TODOS los privilegios.
/// 3. Si llegan <paramref name="AdminEmail"/> y <paramref name="AdminPassword"/>, crea ese usuario
///    (si falta; si ya existe NO cambia su password) y le asegura el rol Administrador.
///
/// Resuelve el "huevo y la gallina": como todos los endpoints piden privilegios, alguien tiene que
/// tenerlos desde el principio para crear empresas, roles y usuarios.
/// </summary>
public record SembrarSeguridadCommand(string? AdminEmail = null, string? AdminPassword = null) : IRequest;

public class SembrarSeguridadCommandHandler : IRequestHandler<SembrarSeguridadCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ILogger<SembrarSeguridadCommandHandler> _logger;

    public SembrarSeguridadCommandHandler(
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher,
        ILogger<SembrarSeguridadCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _logger = logger;
    }

    public async Task Handle(SembrarSeguridadCommand request, CancellationToken cancellationToken)
    {
        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            await SembrarCatalogoAsync(cancellationToken);
            var administrador = await SembrarRolAdministradorAsync(cancellationToken);
            await SembrarUsuarioAdministradorAsync(request, administrador, cancellationToken);

            await _unitOfWork.CommitTransactionAsync(cancellationToken);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    private async Task SembrarCatalogoAsync(CancellationToken cancellationToken)
    {
        var existentes = (await _unitOfWork.Privilegios.GetAllAsync(cancellationToken: cancellationToken))
            .Select(p => p.Clave)
            .ToHashSet();

        var faltantes = Permisos.Catalogo.Where(d => !existentes.Contains(d.Clave)).ToList();

        foreach (var definicion in faltantes)
        {
            await _unitOfWork.Privilegios.AddAsync(
                Privilegio.Crear(definicion.Clave, definicion.Nombre, definicion.Descripcion, definicion.Ambito),
                cancellationToken);
        }

        if (faltantes.Count > 0)
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Seguridad: se crearon {Cantidad} privilegios del catálogo.", faltantes.Count);
        }
    }

    private async Task<Rol> SembrarRolAdministradorAsync(CancellationToken cancellationToken)
    {
        var administrador = await _unitOfWork.Roles.GetByNombreAsync(NombresRol.AdministradorPlataforma, null, cancellationToken);

        if (administrador is null)
        {
            administrador = Rol.Crear(
                NombresRol.AdministradorPlataforma,
                "Rol global de la plataforma con todos los privilegios.",
                activo: true,
                empresaId: null);

            await _unitOfWork.Roles.AddAsync(administrador, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Seguridad: se creó el rol global {Rol}.", administrador.Nombre);
        }

        var asignados = (await _unitOfWork.RolPrivilegios.GetAllAsync(administrador.RolId, cancellationToken: cancellationToken))
            .Select(rp => rp.PrivilegioId)
            .ToHashSet();

        var porAsignar = (await _unitOfWork.Privilegios.GetAllAsync(cancellationToken: cancellationToken))
            .Where(p => !asignados.Contains(p.PrivilegioId))
            .ToList();

        foreach (var privilegio in porAsignar)
            await _unitOfWork.RolPrivilegios.AddAsync(RolPrivilegio.Crear(administrador.RolId, privilegio.PrivilegioId), cancellationToken);

        if (porAsignar.Count > 0)
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Seguridad: se asignaron {Cantidad} privilegios al rol {Rol}.", porAsignar.Count, administrador.Nombre);
        }

        return administrador;
    }

    private async Task SembrarUsuarioAdministradorAsync(SembrarSeguridadCommand request, Rol administrador, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.AdminEmail) || string.IsNullOrWhiteSpace(request.AdminPassword))
            return;

        var email = request.AdminEmail.Trim().ToLowerInvariant();
        var usuario = await _unitOfWork.Usuarios.GetByEmailAsync(email, cancellationToken);

        if (usuario is null)
        {
            usuario = Usuario.Crear(email, _passwordHasher.Hash(request.AdminPassword));

            await _unitOfWork.Usuarios.AddAsync(usuario, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Seguridad: se creó el usuario administrador {Email}.", email);
        }

        if (!await _unitOfWork.UsuarioRoles.AsignacionExistsAsync(usuario.UsuarioId, administrador.RolId, cancellationToken: cancellationToken))
        {
            await _unitOfWork.UsuarioRoles.AddAsync(UsuarioRol.Crear(usuario.UsuarioId, administrador.RolId), cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Seguridad: se asignó el rol {Rol} a {Email}.", administrador.Nombre, email);
        }
    }
}
