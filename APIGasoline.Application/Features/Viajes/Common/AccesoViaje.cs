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
    /// Pausar, reanudar o finalizar: el operador del viaje o, para el de otro, quien tenga
    /// viaje.corregir_cerrado (supervisor).
    /// </summary>
    public static async Task ValidarOperadorAsync(IUsuarioActual usuario, Viaje viaje, CancellationToken cancellationToken)
    {
        await usuario.ValidarEmpresaAsync(viaje.EmpresaId, cancellationToken);
        await usuario.ValidarPropioOPrivilegioAsync(viaje.EmpleadoOperadorId == usuario.EmpleadoId, Permisos.ViajeCorregirCerrado, cancellationToken);
    }
}
