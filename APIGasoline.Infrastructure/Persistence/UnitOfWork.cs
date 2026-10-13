using System;
using System.Collections.Generic;
using System.Text;
using APIGasoline.Domain.Interfaces;
using APIGasoline.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore.Storage;

namespace APIGasoline.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;
    private IDbContextTransaction? _transaction;

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
        Usuarios = new UsuarioRepository(context);
        Roles = new RolRepository(context);
        Privilegios = new PrivilegioRepository(context);
        UsuarioRoles = new UsuarioRolRepository(context);
        RolPrivilegios = new RolPrivilegioRepository(context);
        Corporativos = new CorporativoRepository(context);
        Divisiones = new DivisionRepository(context);
        Empresas = new EmpresaRepository(context);
        CentrosCosto = new CentroCostoRepository(context);
        Empleados = new EmpleadoRepository(context);
        Carros = new CarroRepository(context);
        SolicitudesReserva = new SolicitudReservaRepository(context);
        Viajes = new ViajeRepository(context);
        PuntosRuta = new PuntoRutaRepository(context);
        RutasProcesadas = new RutaProcesadaRepository(context);
        CargasCombustible = new CargaCombustibleRepository(context);
        CambiosEstatusCarro = new CambioEstatusCarroRepository(context);
        Notificaciones = new NotificacionRepository(context);
        OutboxMensajes = new OutboxMensajeRepository(context);
        Dispositivos = new DispositivoEmpleadoRepository(context);
        AuditoriaEventos = new AuditoriaEventoRepository(context);
        RevisionesAuditoria = new RevisionAuditoriaRepository(context);
    }

    public IUsuarioRepository Usuarios { get; }
    public IRolRepository Roles { get; }
    public IPrivilegioRepository Privilegios { get; }
    public IUsuarioRolRepository UsuarioRoles { get; }
    public IRolPrivilegioRepository RolPrivilegios { get; }
    public ICorporativoRepository Corporativos { get; }
    public IDivisionRepository Divisiones { get; }
    public IEmpresaRepository Empresas { get; }
    public ICentroCostoRepository CentrosCosto { get; }
    public IEmpleadoRepository Empleados { get; }
    public ICarroRepository Carros { get; }
    public ISolicitudReservaRepository SolicitudesReserva { get; }
    public IViajeRepository Viajes { get; }
    public IPuntoRutaRepository PuntosRuta { get; }
    public IRutaProcesadaRepository RutasProcesadas { get; }
    public ICargaCombustibleRepository CargasCombustible { get; }
    public ICambioEstatusCarroRepository CambiosEstatusCarro { get; }
    public INotificacionRepository Notificaciones { get; }
    public IOutboxMensajeRepository OutboxMensajes { get; }
    public IDispositivoEmpleadoRepository Dispositivos { get; }
    public IAuditoriaEventoRepository AuditoriaEventos { get; }
    public IRevisionAuditoriaRepository RevisionesAuditoria { get; }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => _context.SaveChangesAsync(cancellationToken);

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        _transaction ??= await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction is null) return;

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
            await _transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await RollbackTransactionAsync(cancellationToken);
            throw;
        }
        finally
        {
            await DisposeTransactionAsync();
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction is null) return;

        await _transaction.RollbackAsync(cancellationToken);
        await DisposeTransactionAsync();
    }

    private async Task DisposeTransactionAsync()
    {
        if (_transaction is not null)
        {
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
    }
}