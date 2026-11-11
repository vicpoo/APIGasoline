using System;
using System.Collections.Generic;
using System.Text;

using APIGasoline.Domain.Entities;
using APIGasoline.Infrastructure.Persistence.Converters;
using Microsoft.EntityFrameworkCore;

namespace APIGasoline.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Rol> Roles => Set<Rol>();
    public DbSet<Privilegio> Privilegios => Set<Privilegio>();
    public DbSet<UsuarioRol> UsuarioRoles => Set<UsuarioRol>();
    public DbSet<RolPrivilegio> RolPrivilegios => Set<RolPrivilegio>();
    public DbSet<Corporativo> Corporativos => Set<Corporativo>();
    public DbSet<Division> Divisiones => Set<Division>();
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
        base.OnModelCreating(modelBuilder);
    }
}