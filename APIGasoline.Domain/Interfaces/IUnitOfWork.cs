namespace APIGasoline.Domain.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IUsuarioRepository Usuarios { get; }
    IRolRepository Roles { get; }
    IPrivilegioRepository Privilegios { get; }
    IUsuarioRolRepository UsuarioRoles { get; }
    IRolPrivilegioRepository RolPrivilegios { get; }
    ICorporativoRepository Corporativos { get; }
    IDivisionRepository Divisiones { get; }
    ICorporativoDivisionRepository CorporativoDivisiones { get; }
    IEmpresaRepository Empresas { get; }
    ICentroCostoRepository CentrosCosto { get; }
    IEmpleadoRepository Empleados { get; }
    ICarroRepository Carros { get; }
    ISolicitudReservaRepository SolicitudesReserva { get; }
    IViajeRepository Viajes { get; }
    IViajePausaRepository ViajePausas { get; }
    IPuntoRutaRepository PuntosRuta { get; }
    IRutaProcesadaRepository RutasProcesadas { get; }
    ICargaCombustibleRepository CargasCombustible { get; }
    ICambioEstatusCarroRepository CambiosEstatusCarro { get; }
    INotificacionRepository Notificaciones { get; }
    IOutboxMensajeRepository OutboxMensajes { get; }
    IDispositivoEmpleadoRepository Dispositivos { get; }

    /// <summary>Sesiones de los usuarios (refresh tokens).</summary>
    ISesionUsuarioRepository Sesiones { get; }
    IAuditoriaEventoRepository AuditoriaEventos { get; }
    IRevisionAuditoriaRepository RevisionesAuditoria { get; }
    IRevisionCombustibleRepository RevisionCombustible { get; }

    /// <summary>Lecturas para los reportes de la flota (PDF / Excel).</summary>
    IReporteFlotaRepository ReportesFlota { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Como <see cref="SaveChangesAsync"/>, pero si choca con otra petición simultánea devuelve false
    /// y descarta los cambios pendientes, para que el handler vuelva a leer y lo intente otra vez
    /// (por ejemplo, responder como reintento idempotente). Choques que cubre:
    /// - Un índice único lo rechazó (otra petición guardó lo mismo un instante antes).
    /// - SQL Server eligió esta transacción como víctima de un interbloqueo (error 1205) y la deshizo.
    /// Cualquier otro error se lanza igual que en <see cref="SaveChangesAsync"/>.
    /// </summary>
    Task<bool> SaveChangesSalvoConcurrenciaAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}
