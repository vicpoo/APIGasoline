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
    IEmpresaRepository Empresas { get; }
    ICentroCostoRepository CentrosCosto { get; }
    IEmpleadoRepository Empleados { get; }
    ICarroRepository Carros { get; }
    ISolicitudReservaRepository SolicitudesReserva { get; }
    IViajeRepository Viajes { get; }
    IPuntoRutaRepository PuntosRuta { get; }
    IRutaProcesadaRepository RutasProcesadas { get; }
    ICargaCombustibleRepository CargasCombustible { get; }
    ICambioEstatusCarroRepository CambiosEstatusCarro { get; }
    INotificacionRepository Notificaciones { get; }
    IOutboxMensajeRepository OutboxMensajes { get; }
    IDispositivoEmpleadoRepository Dispositivos { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}
