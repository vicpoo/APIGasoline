using APIGasoline.Application.Common.Interfaces;
using APIGasoline.Application.Features.Viajes.DTOs;
using APIGasoline.Domain.Interfaces;

namespace APIGasoline.Application.Features.Viajes.Common;

/// <summary>
/// Agrega el nombre y la foto del operador a los viajes (tarjeta del Historial).
/// Una sola consulta de empleados para toda la lista; así quien tiene viaje.ver_todos
/// ve quién manejó sin necesitar empleado.ver.
/// </summary>
public static class OperadorViaje
{
    public static async Task<IReadOnlyList<ViajeDto>> ConOperadorAsync(
        this IReadOnlyList<ViajeDto> viajes,
        IUnitOfWork unitOfWork,
        IFileStorageService storage,
        CancellationToken cancellationToken)
    {
        if (viajes.Count == 0)
            return viajes;

        var empleados = await unitOfWork.Empleados.GetByIdsAsync(viajes.Select(v => v.EmpleadoOperadorId), cancellationToken);

        var operadores = new Dictionary<Guid, OperadorViajeDto>();
        foreach (var empleado in empleados)
        {
            operadores[empleado.EmpleadoId] = new OperadorViajeDto(
                empleado.EmpleadoId,
                empleado.NombreCompleto,
                await storage.ObtenerUrlAsync(empleado.FotoPerfil, cancellationToken));
        }

        return viajes
            .Select(v => v with { Operador = operadores.GetValueOrDefault(v.EmpleadoOperadorId) })
            .ToList();
    }

    public static async Task<ViajeDto> ConOperadorAsync(
        this ViajeDto viaje,
        IUnitOfWork unitOfWork,
        IFileStorageService storage,
        CancellationToken cancellationToken)
        => (await new[] { viaje }.ConOperadorAsync(unitOfWork, storage, cancellationToken))[0];
}
