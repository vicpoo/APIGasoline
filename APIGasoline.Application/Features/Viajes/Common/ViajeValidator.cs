using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Application.Features.CentrosCosto.Common;
using APIGasoline.Domain.Entities;
using APIGasoline.Domain.Enums;
using APIGasoline.Domain.Interfaces;

namespace APIGasoline.Application.Features.Viajes.Common;

/// <summary>
/// Validaciones contra otras entidades al registrar un viaje (en línea o sincronizado).
/// Devuelve el carro (rastreado) para poder actualizar su kilometraje.
/// </summary>
internal static class ViajeValidator
{
    public static async Task<Carro> ValidarInicioAsync(IUnitOfWork unitOfWork, Viaje viaje, bool esSincronizacion, CancellationToken cancellationToken)
    {
        if (!await unitOfWork.Empresas.ExistsAsync(viaje.EmpresaId, cancellationToken))
            throw new NotFoundException($"No existe una empresa con el id {viaje.EmpresaId}.");

        var carro = await unitOfWork.Carros.GetByIdAsync(viaje.CarroId, cancellationToken)
            ?? throw new NotFoundException($"No existe un carro con el id {viaje.CarroId}.");

        if (carro.EmpresaId != viaje.EmpresaId)
            throw new BadRequestException($"El carro {carro.CarroId} no pertenece a la empresa {viaje.EmpresaId}.");

        var operador = await unitOfWork.Empleados.GetByIdAsync(viaje.EmpleadoOperadorId, cancellationToken)
            ?? throw new NotFoundException($"No existe un empleado con el id {viaje.EmpleadoOperadorId}.");

        if (operador.EmpresaId != viaje.EmpresaId)
            throw new BadRequestException($"El empleado {operador.EmpleadoId} no pertenece a la empresa {viaje.EmpresaId}.");

        if (viaje.SolicitudReservaId is not null)
        {
            var solicitud = await unitOfWork.SolicitudesReserva.GetByIdAsync(viaje.SolicitudReservaId.Value, cancellationToken)
                ?? throw new NotFoundException($"No existe una solicitud de reserva con el id {viaje.SolicitudReservaId}.");

            if (solicitud.CarroId != viaje.CarroId || solicitud.EmpresaId != viaje.EmpresaId)
                throw new BadRequestException("La solicitud de reserva no corresponde al carro y empresa del viaje.");

            if (solicitud.EmpleadoSolicitanteId != viaje.EmpleadoOperadorId)
                throw new BadRequestException("El operador del viaje debe ser el empleado que tiene la reserva.");

            // En línea la reserva debe seguir aprobada; un viaje offline pudo hacerse
            // antes de que la reserva se cancelara, así que también se acepta "cancelada".
            var estadoValido = solicitud.Estado == EstadoSolicitudReserva.Aprobada
                || (esSincronizacion && solicitud.Estado == EstadoSolicitudReserva.Cancelada);

            if (!estadoValido)
                throw new ConflictException($"La solicitud de reserva no está aprobada (estado actual: {solicitud.Estado}).");
        }
        else if (!esSincronizacion && !carro.EstaDisponible)
        {
            // Sin reserva, el carro debe estar libre
            throw new ConflictException($"El carro {carro.CarroId} no está disponible (estado actual: {carro.Estado}).");
        }

        await CentroCostoValidator.ValidarAsync(unitOfWork, viaje.EmpresaId, viaje.CentroCostoId, cancellationToken);

        if (viaje.EstaEnCurso && await unitOfWork.Viajes.ExisteEnCursoPorCarroAsync(viaje.CarroId, cancellationToken))
            throw new ConflictException($"El carro {viaje.CarroId} ya tiene un viaje en curso.");

        return carro;
    }
}
