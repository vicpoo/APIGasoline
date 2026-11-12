using APIGasoline.Application.Common.Interfaces;
using APIGasoline.Application.Common.Security;
using APIGasoline.Domain.Entities;

namespace APIGasoline.Application.Features.Viajes.Common;

/// <summary>Quién puede ver o mover un viaje en particular (se usa en los handlers).</summary>
public static class AccesoViaje
{
    /// <summary>
    /// Ver un viaje: de su empresa y, sin viaje.ver_todos, solo si es el operador del viaje.
    /// </summary>
    public static async Task ValidarLecturaAsync(IUsuarioActual usuario, Viaje viaje, CancellationToken cancellationToken)
    {
        await usuario.ValidarEmpresaAsync(viaje.EmpresaId, cancellationToken);
        await usuario.ValidarPropioOPrivilegioAsync(viaje.EmpleadoOperadorId == usuario.EmpleadoId, Permisos.ViajeVerTodos, cancellationToken);
    }

    /// <summary>
    /// Ver el recorrido o las cargas de un viaje (puntos GPS, GPX, ruta procesada, mapa, carga por id):
    /// de su empresa y, si no es el operador del viaje, con viaje.ver_todos (supervisión),
    /// auditoria.ver (auditoría) u otro de <paramref name="otrosPrivilegios"/>.
    /// </summary>
    public static async Task ValidarLecturaDetalleAsync(
        IUsuarioActual usuario,
        Viaje viaje,
        CancellationToken cancellationToken,
        params string[] otrosPrivilegios)
    {
        await usuario.ValidarEmpresaAsync(viaje.EmpresaId, cancellationToken);
        await usuario.ValidarPropioOAlgunPrivilegioAsync(
            viaje.EmpleadoOperadorId == usuario.EmpleadoId,
            [Permisos.ViajeVerTodos, Permisos.AuditoriaVer, .. otrosPrivilegios],
            "Solo puedes ver el recorrido y las cargas de tus propios viajes",
            cancellationToken);
    }

    /// <summary>
    /// Pausar, reanudar, finalizar, mandar puntos GPS o subir la foto del tablero: el operador del
    /// viaje o, para el de otro, quien tenga viaje.corregir_cerrado (supervisor).
    /// </summary>
    public static async Task ValidarOperadorAsync(IUsuarioActual usuario, Viaje viaje, CancellationToken cancellationToken)
    {
        await usuario.ValidarEmpresaAsync(viaje.EmpresaId, cancellationToken);
        await usuario.ValidarPropioOPrivilegioAsync(viaje.EmpleadoOperadorId == usuario.EmpleadoId, Permisos.ViajeCorregirCerrado, cancellationToken);
    }
}
