using System;
using System.Collections.Generic;
using System.Text;

using APIGasoline.Domain.Entities;
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
    public DbSet<AuditoriaEvento> AuditoriaEventos => Set<AuditoriaEvento>();
    public DbSet<RevisionAuditoria> RevisionesAuditoria => Set<RevisionAuditoria>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}