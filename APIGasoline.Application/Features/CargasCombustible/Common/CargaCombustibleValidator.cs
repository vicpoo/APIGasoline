using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Application.Common.Interfaces;
using APIGasoline.Application.Features.Viajes.Common;
using APIGasoline.Domain.Entities;
using APIGasoline.Domain.Interfaces;

namespace APIGasoline.Application.Features.CargasCombustible.Common;

/// <summary>
/// Validaciones contra el viaje y el carro, compartidas entre Create y Update.
/// </summary>
internal static class CargaCombustibleValidator
{
    public static async Task ValidarAsync(
        IUnitOfWork unitOfWork,
        IDateTimeProvider dateTime,
        Viaje viaje,
        CargaCombustible carga,
        CancellationToken cancellationToken)
    {
        if (!VentanaViaje.Contiene(carga.FechaCarga, viaje.FechaInicio, viaje.FechaFin, dateTime.Now))
            throw new BadRequestException("La fecha de carga está fuera del periodo del viaje.");

        // No se pueden cargar más litros de los que caben en el tanque
        var carro = await unitOfWork.Carros.GetByIdAsync(viaje.CarroId, cancellationToken)
            ?? throw new NotFoundException($"No existe un carro con el id {viaje.CarroId}.");

        if (ReglaCapacidadTanque.Excede(carga.Litros, carro.CapacidadTanque))
            throw new BadRequestException(
                $"Los litros ({carga.Litros}) exceden la capacidad del tanque del carro ({carro.CapacidadTanque}).");

        var excludeId = carga.CargaCombustibleId == 0 ? (int?)null : carga.CargaCombustibleId;

        if (await unitOfWork.CargasCombustible.ExisteEnFechaAsync(viaje.ViajeId, carga.FechaCarga, excludeId, cancellationToken))
            throw new ConflictException("Ya existe una carga registrada para este viaje en la misma fecha y hora.");
    }
}
