using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Application.Common.Interfaces;
using APIGasoline.Application.Features.CambiosEstatusCarro.DTOs;
using APIGasoline.Domain.Entities;
using APIGasoline.Domain.Enums;
using APIGasoline.Domain.Interfaces;
using AutoMapper;
using MediatR;

namespace APIGasoline.Application.Features.CambiosEstatusCarro.Commands;

/// <summary>
/// Cambio manual del estatus de un carro (ej. mandarlo a mantenimiento o regresarlo a disponible).
/// Cambia el carro y registra la bitácora en un solo SaveChanges.
/// Si se indica un viaje afectado que está en curso, el viaje se interrumpe.
/// </summary>
public record CambiarEstatusCarroCommand(
    Guid CarroId,
    Guid EmpleadoId,
    EstadoCarro EstatusNuevo,
    string? Motivo,
    Guid? ViajeAfectadoId,
    DateTime? FechaCambio,
    bool Sincronizado) : IRequest<CambioEstatusCarroDto>;

public class CambiarEstatusCarroCommandHandler : IRequestHandler<CambiarEstatusCarroCommand, CambioEstatusCarroDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IDateTimeProvider _dateTime;

    public CambiarEstatusCarroCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, IDateTimeProvider dateTime)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _dateTime = dateTime;
    }

    public async Task<CambioEstatusCarroDto> Handle(CambiarEstatusCarroCommand request, CancellationToken cancellationToken)
    {
        // "Ocupado" solo lo asigna el flujo de aprobación de reservas
        if (request.EstatusNuevo == EstadoCarro.Ocupado)
            throw new BadRequestException("El estatus 'ocupado' se asigna al aprobar una solicitud de reserva.");

        var carro = await _unitOfWork.Carros.GetByIdAsync(request.CarroId, cancellationToken)
            ?? throw new NotFoundException($"No existe un carro con el id {request.CarroId}.");

        var empleado = await _unitOfWork.Empleados.GetByIdAsync(request.EmpleadoId, cancellationToken)
            ?? throw new NotFoundException($"No existe un empleado con el id {request.EmpleadoId}.");

        if (empleado.EmpresaId != carro.EmpresaId)
            throw new BadRequestException($"El empleado {empleado.EmpleadoId} no pertenece a la empresa del carro.");

        Viaje? viajeAfectado = null;

        if (request.ViajeAfectadoId is not null)
        {
            viajeAfectado = await _unitOfWork.Viajes.GetByIdAsync(request.ViajeAfectadoId.Value, cancellationToken)
                ?? throw new NotFoundException($"No existe un viaje con el id {request.ViajeAfectadoId}.");

            if (viajeAfectado.CarroId != carro.CarroId)
                throw new BadRequestException($"El viaje {viajeAfectado.ViajeId} no corresponde al carro {carro.CarroId}.");
        }
        else if (request.EstatusNuevo == EstadoCarro.EnMantenimiento
                 && await _unitOfWork.Viajes.ExisteEnCursoPorCarroAsync(carro.CarroId, cancellationToken))
        {
            throw new ConflictException("El carro tiene un viaje en curso. Indica el viaje afectado para interrumpirlo.");
        }

        // El dominio valida la transición, cambia el carro y crea el registro
        var cambio = CambioEstatusCarro.Registrar(
            carro,
            empleado.EmpleadoId,
            request.EstatusNuevo,
            request.Motivo,
            request.ViajeAfectadoId,
            request.FechaCambio,
            request.Sincronizado);

        if (viajeAfectado is { EstaEnCurso: true })
        {
            viajeAfectado.Interrumpir(
                cambio.Motivo ?? $"El carro cambió a estatus {cambio.EstatusNuevo}.",
                request.FechaCambio ?? _dateTime.Now,
                null,
                null,
                null);
        }

        // Quién tiene el carro asignado (reserva aprobada vigente), para avisarle
        var reservaVigente = await _unitOfWork.SolicitudesReserva.GetAprobadaByCarroAsync(carro.CarroId, cancellationToken);

        // Transacción: se guarda el cambio (para tener su id) y luego la notificación que lo referencia
        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            await _unitOfWork.CambiosEstatusCarro.AddAsync(cambio, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            if (reservaVigente is not null && reservaVigente.EmpleadoSolicitanteId != empleado.EmpleadoId)
            {
                await _unitOfWork.Notificaciones.AddAsync(Notificacion.Crear(
                    carro.EmpresaId,
                    reservaVigente.EmpleadoSolicitanteId,
                    TipoNotificacion.CambioEstatusForzado,
                    cambio.CambioEstatusCarroId,
                    $"El carro {carro.Placas} cambió a {cambio.EstatusNuevo}. Motivo: {cambio.Motivo ?? "sin motivo"}"), cancellationToken);
            }

            await _unitOfWork.CommitTransactionAsync(cancellationToken);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }

        return _mapper.Map<CambioEstatusCarroDto>(cambio);
    }
}
