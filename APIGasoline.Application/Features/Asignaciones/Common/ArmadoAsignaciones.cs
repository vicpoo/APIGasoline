using APIGasoline.Application.Features.Asignaciones.DTOs;
using APIGasoline.Application.Features.SolicitudesReserva.DTOs;
using APIGasoline.Domain.Entities;

namespace APIGasoline.Application.Features.Asignaciones.Common;

/// <summary>
/// Arma la vista Asignaciones a partir de datos ya cargados (sin BD ni red, se prueba sola).
/// - Carros: todos los que no están dados de baja, ordenados por placas.
/// - Empleados: todos, primero los que tienen carro y luego por nombre.
/// - Una asignación es una reserva aprobada; si el carro tuviera más de una (dato inconsistente)
///   se toma la más reciente.
/// </summary>
public static class ArmadoAsignaciones
{
    /// <param name="reservasAprobadas">Reservas en estado aprobada de la empresa.</param>
    /// <param name="reservasConViajeEnCurso">Ids de las reservas que tienen un viaje en curso (o en pausa).</param>
    /// <param name="url">Convierte la llave de una foto en su URL (ya firmada).</param>
    public static AsignacionesDto Armar(
        IReadOnlyList<Carro> carros,
        IReadOnlyList<Empleado> empleados,
        IReadOnlyList<SolicitudReserva> reservasAprobadas,
        IReadOnlySet<Guid> reservasConViajeEnCurso,
        Func<string?, string?> url)
    {
        ArgumentNullException.ThrowIfNull(carros);
        ArgumentNullException.ThrowIfNull(empleados);
        ArgumentNullException.ThrowIfNull(reservasAprobadas);
        ArgumentNullException.ThrowIfNull(reservasConViajeEnCurso);
        ArgumentNullException.ThrowIfNull(url);

        var empleadosPorId = empleados.ToDictionary(e => e.EmpleadoId);
        var carrosPorId = carros.ToDictionary(c => c.CarroId);

        // Una reserva vigente por carro (la más reciente)
        var reservaPorCarro = reservasAprobadas
            .GroupBy(r => r.CarroId)
            .ToDictionary(g => g.Key, g => g.OrderByDescending(r => r.FechaResolucion ?? r.FechaSolicitud).First());

        var listaCarros = carros
            .Where(c => !c.DadoDeBaja)
            .OrderBy(c => c.Placas, StringComparer.OrdinalIgnoreCase)
            .Select(c =>
            {
                AsignacionCarroDto? asignacion = null;
                if (reservaPorCarro.TryGetValue(c.CarroId, out var reserva)
                    && empleadosPorId.TryGetValue(reserva.EmpleadoSolicitanteId, out var empleado))
                {
                    asignacion = new AsignacionCarroDto(
                        reserva.SolicitudReservaId,
                        new EmpleadoResumenDto(empleado.EmpleadoId, empleado.NombreCompleto, url(empleado.FotoPerfil)),
                        reserva.FechaResolucion,
                        reservasConViajeEnCurso.Contains(reserva.SolicitudReservaId));
                }

                return new CarroAsignacionDto(
                    c.CarroId, c.Marca, c.Modelo, c.Anio, c.Color, c.Placas, url(c.FotoVehiculo), c.Estado, asignacion);
            })
            .ToList();

        var reservasPorEmpleado = reservaPorCarro.Values
            .GroupBy(r => r.EmpleadoSolicitanteId)
            .ToDictionary(g => g.Key, g => g.ToList());

        var listaEmpleados = empleados
            .Select(e =>
            {
                var asignados = reservasPorEmpleado.TryGetValue(e.EmpleadoId, out var reservas)
                    ? reservas
                        .Where(r => carrosPorId.TryGetValue(r.CarroId, out var carro) && !carro.DadoDeBaja)
                        .Select(r =>
                        {
                            var carro = carrosPorId[r.CarroId];
                            return new AsignacionEmpleadoDto(
                                r.SolicitudReservaId,
                                new CarroResumenDto(carro.CarroId, carro.Marca, carro.Modelo, carro.Anio, carro.Color, carro.Placas, url(carro.FotoVehiculo)),
                                carro.Estado,
                                r.FechaResolucion,
                                reservasConViajeEnCurso.Contains(r.SolicitudReservaId));
                        })
                        .OrderBy(a => a.Carro.Placas, StringComparer.OrdinalIgnoreCase)
                        .ToList()
                    : [];

                return new EmpleadoAsignacionDto(e.EmpleadoId, e.NombreCompleto, url(e.FotoPerfil), asignados);
            })
            .OrderByDescending(e => e.Carros.Count > 0)
            .ThenBy(e => e.NombreCompleto, StringComparer.CurrentCultureIgnoreCase)
            .ToList();

        return new AsignacionesDto(listaCarros, listaEmpleados);
    }
}
