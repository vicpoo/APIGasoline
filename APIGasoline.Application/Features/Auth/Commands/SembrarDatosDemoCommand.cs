using APIGasoline.Application.Common.Interfaces;
using APIGasoline.Application.Common.Security;
using APIGasoline.Domain.Entities;
using APIGasoline.Domain.Enums;
using APIGasoline.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace APIGasoline.Application.Features.Auth.Commands;

/// <summary>
/// Datos de demostración para DESARROLLO (la API solo lo manda en Development con Seguridad:SembrarDemo = true).
/// Deja dos empresas aisladas para probar en Postman y, después, las pruebas de fugas entre empresas:
///
/// <code>
/// Empresa Demo A                         Empresa Demo B
///  ├─ admin.a@demo.local  (Administrador de empresa)   ├─ admin.b@demo.local
///  ├─ operador.a@demo.local (Operador, con licencia)   ├─ operador.b@demo.local
///  ├─ centro de costo CC-A-01             ├─ centro de costo CC-B-01
///  └─ carros DEMO-A01, DEMO-A02           └─ carros DEMO-B01, DEMO-B02
/// </code>
///
/// Todos los usuarios demo usan la contraseña <see cref="PasswordDemo"/>.
/// El usuario de plataforma (ve todas las empresas) es el administrador de Seguridad:AdminEmail.
///
/// Idempotente: si ya existe admin.a@demo.local no hace nada. Debe correr después de
/// <see cref="SembrarSeguridadCommand"/> (necesita el catálogo de privilegios).
/// </summary>
public record SembrarDatosDemoCommand : IRequest;

public class SembrarDatosDemoCommandHandler : IRequestHandler<SembrarDatosDemoCommand>
{
    public const string PasswordDemo = "Demo#Gasolina2026";
    public const string RolAdministradorEmpresa = "Administrador de empresa";
    public const string RolOperador = "Operador";

    /// <summary>Privilegios del rol Operador: pedir carros, registrar viajes y cargas.</summary>
    private static readonly string[] PrivilegiosOperador =
    [
        Permisos.CarroVer,
        Permisos.CarroVerDetalle,
        Permisos.CentroCostoVer,
        Permisos.SolicitudCrear,
        Permisos.SolicitudVerPropias,
        Permisos.ViajeRegistrar,
        Permisos.ViajeVerPropios,
        Permisos.ViajeVerDetalle,
        Permisos.CargaRegistrar,
    ];

    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IDateTimeProvider _dateTime;
    private readonly ILogger<SembrarDatosDemoCommandHandler> _logger;

    public SembrarDatosDemoCommandHandler(
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher,
        IDateTimeProvider dateTime,
        ILogger<SembrarDatosDemoCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _dateTime = dateTime;
        _logger = logger;
    }

    public async Task Handle(SembrarDatosDemoCommand request, CancellationToken cancellationToken)
    {
        if (await _unitOfWork.Usuarios.GetByEmailAsync("admin.a@demo.local", cancellationToken) is not null)
            return;

        var privilegios = await _unitOfWork.Privilegios.GetAllAsync(cancellationToken: cancellationToken);
        if (privilegios.Count == 0)
            throw new InvalidOperationException("Primero hay que sembrar la seguridad (catálogo de privilegios).");

        // Un solo hash para todos: PBKDF2 es lento a propósito
        var passwordHash = _passwordHasher.Hash(PasswordDemo);

        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            await SembrarEmpresaAsync("A", privilegios, passwordHash, cancellationToken);
            await SembrarEmpresaAsync("B", privilegios, passwordHash, cancellationToken);

            await _unitOfWork.CommitTransactionAsync(cancellationToken);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }

        _logger.LogInformation(
            "Datos demo: empresas A y B creadas. Usuarios admin.a@demo.local, operador.a@demo.local, admin.b@demo.local y operador.b@demo.local (contraseña {Password}).",
            PasswordDemo);
    }

    private async Task SembrarEmpresaAsync(string letra, IReadOnlyList<Privilegio> privilegios, string passwordHash, CancellationToken cancellationToken)
    {
        var empresa = Empresa.Crear($"Empresa Demo {letra}", corporativoId: null, divisionId: null);
        await _unitOfWork.Empresas.AddAsync(empresa, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Roles propios de la empresa: solo pueden llevar privilegios de ámbito Empresa
        var administrador = Rol.Crear(RolAdministradorEmpresa, "Administra todo lo de su empresa (demo).", activo: true, empresaId: empresa.EmpresaId);
        var operador = Rol.Crear(RolOperador, "Pide carros y registra viajes y cargas (demo).", activo: true, empresaId: empresa.EmpresaId);
        await _unitOfWork.Roles.AddAsync(administrador, cancellationToken);
        await _unitOfWork.Roles.AddAsync(operador, cancellationToken);

        foreach (var privilegio in privilegios.Where(p => p.Ambito == AmbitoPrivilegio.Empresa))
            await _unitOfWork.RolPrivilegios.AddAsync(RolPrivilegio.Crear(administrador.RolId, privilegio.PrivilegioId), cancellationToken);

        foreach (var privilegio in privilegios.Where(p => PrivilegiosOperador.Contains(p.Clave)))
            await _unitOfWork.RolPrivilegios.AddAsync(RolPrivilegio.Crear(operador.RolId, privilegio.PrivilegioId), cancellationToken);

        var minuscula = letra.ToLowerInvariant();
        await CrearEmpleadoAsync(empresa, $"admin.{minuscula}@demo.local", $"Admin Demo {letra}", administrador, conLicencia: false, passwordHash, cancellationToken);
        await CrearEmpleadoAsync(empresa, $"operador.{minuscula}@demo.local", $"Operador Demo {letra}", operador, conLicencia: true, passwordHash, cancellationToken);

        await _unitOfWork.CentrosCosto.AddAsync(
            CentroCosto.Crear(empresa.EmpresaId, $"Operaciones {letra}", $"CC-{letra}-01"),
            cancellationToken);

        var anioActual = _dateTime.Now.Year;
        await _unitOfWork.Carros.AddAsync(
            Carro.Crear(empresa.EmpresaId, "Nissan", "Versa", 2022, "Blanco", $"DEMO-{letra}01", null, 41m, 35000m, anioActual),
            cancellationToken);
        await _unitOfWork.Carros.AddAsync(
            Carro.Crear(empresa.EmpresaId, "Toyota", "Hilux", 2021, "Gris", $"DEMO-{letra}02", null, 80m, 62000m, anioActual, tipoVehiculo: TipoVehiculo.Camioneta),
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task CrearEmpleadoAsync(
        Empresa empresa,
        string email,
        string nombre,
        Rol rol,
        bool conLicencia,
        string passwordHash,
        CancellationToken cancellationToken)
    {
        var usuario = Usuario.Crear(email, passwordHash);
        await _unitOfWork.Usuarios.AddAsync(usuario, cancellationToken);
        await _unitOfWork.UsuarioRoles.AddAsync(UsuarioRol.Crear(usuario.UsuarioId, rol.RolId), cancellationToken);

        var empleado = Empleado.Crear(empresa.EmpresaId, usuario.UsuarioId, nombre);

        // Sin licencia no se pueden pedir carros. La llave no existe en MinIO (la URL dará 404), es solo demo.
        if (conLicencia)
            empleado.CambiarFotoLicencia($"empresa/{empresa.EmpresaId}/empleado/{empleado.EmpleadoId}/documento/licencia-demo.jpg", _dateTime.Now);

        await _unitOfWork.Empleados.AddAsync(empleado, cancellationToken);
    }
}
