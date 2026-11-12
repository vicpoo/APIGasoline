using APIGasoline.Application.Common.Interfaces;
using APIGasoline.Domain.Entities;
using APIGasoline.Infrastructure.Persistence.Converters;
using Microsoft.EntityFrameworkCore;

namespace APIGasoline.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    private readonly IContextoEmpresa? _contextoEmpresa;

    /// <param name="contextoEmpresa">
    /// Empresa de la petición. Sin él (herramientas de diseño de EF) no se filtra por empresa.
    /// </param>
    public AppDbContext(DbContextOptions<AppDbContext> options, IContextoEmpresa? contextoEmpresa = null) : base(options)
    {
        _contextoEmpresa = contextoEmpresa;
    }

    // ---- Valores que usa el filtro por empresa. EF los vuelve a leer en CADA consulta (son parámetros).

    /// <summary>true: la consulta no se limita a una empresa (plataforma, sistema o sin petición).</summary>
    public bool SinFiltroDeEmpresa => _contextoEmpresa?.VeTodasLasEmpresas ?? true;

    /// <summary>Empresa a la que se limita la consulta (Guid.Empty si el usuario no tiene empresa).</summary>
    public Guid EmpresaDelFiltro => _contextoEmpresa?.EmpresaId ?? Guid.Empty;

    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Rol> Roles => Set<Rol>();
    public DbSet<Privilegio> Privilegios => Set<Privilegio>();
    public DbSet<UsuarioRol> UsuarioRoles => Set<UsuarioRol>();
    public DbSet<RolPrivilegio> RolPrivilegios => Set<RolPrivilegio>();
    public DbSet<Corporativo> Corporativos => Set<Corporativo>();
    public DbSet<Division> Divisiones => Set<Division>();
    public DbSet<CorporativoDivision> CorporativoDivisiones => Set<CorporativoDivision>();
    public DbSet<Empresa> Empresas => Set<Empresa>();
    public DbSet<CentroCosto> CentrosCosto => Set<CentroCosto>();
    public DbSet<Empleado> Empleados => Set<Empleado>();
    public DbSet<Carro> Carros => Set<Carro>();
    public DbSet<SolicitudReserva> SolicitudesReserva => Set<SolicitudReserva>();
    public DbSet<Viaje> Viajes => Set<Viaje>();
    public DbSet<ViajePausa> ViajePausas => Set<ViajePausa>();
    public DbSet<PuntoRuta> PuntosRuta => Set<PuntoRuta>();
    public DbSet<RutaProcesada> RutasProcesadas => Set<RutaProcesada>();
    public DbSet<CargaCombustible> CargasCombustible => Set<CargaCombustible>();
    public DbSet<CambioEstatusCarro> CambiosEstatusCarro => Set<CambioEstatusCarro>();
    public DbSet<Notificacion> Notificaciones => Set<Notificacion>();
    public DbSet<OutboxMensaje> OutboxMensajes => Set<OutboxMensaje>();
    public DbSet<DispositivoEmpleado> Dispositivos => Set<DispositivoEmpleado>();
    public DbSet<SesionUsuario> Sesiones => Set<SesionUsuario>();
    public DbSet<AuditoriaEvento> AuditoriaEventos => Set<AuditoriaEvento>();
    public DbSet<RevisionAuditoria> RevisionesAuditoria => Set<RevisionAuditoria>();

    /// <summary>
    /// Intercalación de la BD: sin distinguir mayúsculas ni acentos (CI_AI), igual que
    /// utf8mb4_0900_ai_ci en MySQL. Afecta búsquedas y los índices únicos (email, placas...).
    /// </summary>
    public const string Intercalacion = "Modern_Spanish_CI_AI";

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        // Todos los Guid (Id y FK) como binary(16) en orden RFC 4122: ver GuidBinarioConverter.
        configurationBuilder.Properties<Guid>()
            .HaveConversion<GuidBinarioConverter>()
            .HaveColumnType(GuidBinarioConverter.ColumnType);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.UseCollation(Intercalacion);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        TiposSql.AplicarRestricciones(modelBuilder);
        AplicarFiltroPorEmpresa(modelBuilder);
        base.OnModelCreating(modelBuilder);
    }

    /// <summary>
    /// Aislamiento entre empresas en TODAS las lecturas: un usuario que no es de la plataforma solo
    /// ve las filas de su empresa (un registro de otra empresa "no existe": 404, y las listas no lo traen).
    /// Las escrituras las revisa <see cref="AislamientoEmpresaInterceptor"/>.
    /// Tablas sin filtro: catálogo de la plataforma (privilegio) y tablas técnicas
    /// (auditoría, outbox, sesiones), que sus handlers validan por su cuenta.
    /// </summary>
    private void AplicarFiltroPorEmpresa(ModelBuilder modelBuilder)
    {
        FiltrarPorColumnaEmpresa<Corporativo>(modelBuilder);
        FiltrarPorColumnaEmpresa<Division>(modelBuilder);
        FiltrarPorColumnaEmpresa<CorporativoDivision>(modelBuilder);
        FiltrarPorColumnaEmpresa<Carro>(modelBuilder);
        FiltrarPorColumnaEmpresa<CentroCosto>(modelBuilder);
        FiltrarPorColumnaEmpresa<Empleado>(modelBuilder);
        FiltrarPorColumnaEmpresa<SolicitudReserva>(modelBuilder);
        FiltrarPorColumnaEmpresa<Viaje>(modelBuilder);
        FiltrarPorColumnaEmpresa<ViajePausa>(modelBuilder);
        FiltrarPorColumnaEmpresa<PuntoRuta>(modelBuilder);
        FiltrarPorColumnaEmpresa<RutaProcesada>(modelBuilder);
        FiltrarPorColumnaEmpresa<CargaCombustible>(modelBuilder);
        FiltrarPorColumnaEmpresa<CambioEstatusCarro>(modelBuilder);
        FiltrarPorColumnaEmpresa<Notificacion>(modelBuilder);
        FiltrarPorColumnaEmpresa<DispositivoEmpleado>(modelBuilder);
        FiltrarPorColumnaEmpresa<RevisionAuditoria>(modelBuilder);

        // La empresa: solo la propia
        modelBuilder.Entity<Empresa>().HasQueryFilter(FiltrosConsulta.Empresa,
            e => SinFiltroDeEmpresa || e.EmpresaId == EmpresaDelFiltro);

        // Roles: los de su empresa y los globales (estos solo se leen; el guardián no deja tocarlos)
        modelBuilder.Entity<Rol>().HasQueryFilter(FiltrosConsulta.Empresa,
            r => SinFiltroDeEmpresa || r.EmpresaId == null || r.EmpresaId == EmpresaDelFiltro);

        // Usuarios: solo las cuentas de su empresa
        modelBuilder.Entity<Usuario>().HasQueryFilter(FiltrosConsulta.Empresa,
            u => SinFiltroDeEmpresa || u.EmpresaId == EmpresaDelFiltro);

        // Asignaciones de roles: las de sus usuarios
        modelBuilder.Entity<UsuarioRol>().HasQueryFilter(FiltrosConsulta.Empresa,
            ur => SinFiltroDeEmpresa || Usuarios.Any(u => u.UsuarioId == ur.UsuarioId && u.EmpresaId == EmpresaDelFiltro));

        // Privilegios de roles: los de los roles que puede ver
        modelBuilder.Entity<RolPrivilegio>().HasQueryFilter(FiltrosConsulta.Empresa,
            rp => SinFiltroDeEmpresa || Roles.Any(r => r.RolId == rp.RolId && (r.EmpresaId == null || r.EmpresaId == EmpresaDelFiltro)));
    }

    private void FiltrarPorColumnaEmpresa<T>(ModelBuilder modelBuilder) where T : class
        => modelBuilder.Entity<T>().HasQueryFilter(FiltrosConsulta.Empresa,
            e => SinFiltroDeEmpresa || EF.Property<Guid>(e, "EmpresaId") == EmpresaDelFiltro);
}