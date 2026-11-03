using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Domain.Entities;
using APIGasoline.Domain.Enums;
using APIGasoline.Domain.Interfaces;

namespace APIGasoline.Application.Features.SolicitudesReserva.Common;

/// <summary>
/// Lógica común para quitarle un carro a un empleado: la usan "Liberar" (el propio empleado deja
/// su carro) y "Desvincular" (un administrador se lo quita desde la vista Asignaciones).
/// 1. No se puede si la reserva tiene un viaje en curso (en pausa también cuenta como en curso).
/// 2. La reserva pasa a "finalizada" (con fecha y observaciones).
/// 3. Si el carro estaba ocupado vuelve a "disponible" y queda en la bitácora como "liberacion".
/// No guarda: quien la llama hace SaveChanges (y la transacción si la necesita).
/// </summary>
internal static class LiberacionReserva
{
    /// <param name="empleadoResponsableId">Quién queda en la bitácora (el empleado o el administrador).</param>
    /// <param name="motivoBitacora">Texto de la bitácora; si no viene se usa uno genérico.</param>
    /// <returns>El cambio de estatus registrado, o null si el carro no estaba ocupado (ej. en mantenimiento).</returns>
    public static async Task<CambioEstatusCarro?> LiberarAsync(
        IUnitOfWork unitOfWork,
        SolicitudReserva solicitud,
        Guid empleadoResponsableId,
        DateTime fecha,
        string? observaciones,
        string? motivoBitacora,
        CancellationToken cancellationToken)
    {
        if (await unitOfWork.Viajes.ExisteEnCursoPorSolicitudAsync(solicitud.SolicitudReservaId, cancellationToken))
            throw new ConflictException("No se puede liberar el carro porque tiene un viaje en curso. Finaliza el viaje primero.");

        // El dominio valida que la reserva esté aprobada
        solicitud.Finalizar(fecha, observaciones);

        var carro = await unitOfWork.Carros.GetByIdAsync(solicitud.CarroId, cancellationToken)
            ?? throw new NotFoundException($"No existe un carro con el id {solicitud.CarroId}.");

        if (carro.Estado != EstadoCarro.Ocupado)
            return null;

        var motivo = motivoBitacora ?? solicitud.ObservacionesLiberacion
            ?? $"Liberado al terminar la reserva {solicitud.SolicitudReservaId}.";

        // La bitácora acepta hasta 255 caracteres
        if (motivo.Length > CambioEstatusCarro.MotivoMaxLength)
            motivo = motivo[..(CambioEstatusCarro.MotivoMaxLength - 3)] + "...";

        var cambio = CambioEstatusCarro.Registrar(
            carro,
            empleadoResponsableId,
            EstadoCarro.Disponible,
            motivo,
            fechaCambio: fecha,
            tipoMotivo: TipoMotivoCambio.Liberacion);

        await unitOfWork.CambiosEstatusCarro.AddAsync(cambio, cancellationToken);
        return cambio;
    }
}
