using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Application.Common.Interfaces;
using APIGasoline.Application.Common.Outbox;
using APIGasoline.Application.Common.Security;
using APIGasoline.Application.Features.SolicitudesReserva.Common;
using APIGasoline.Application.Features.SolicitudesReserva.DTOs;
using APIGasoline.Domain.Entities;
using APIGasoline.Domain.Enums;
using APIGasoline.Domain.Interfaces;
using AutoMapper;
using APIGasoline.Application.Common.Messaging;

namespace APIGasoline.Application.Features.Asignaciones.Commands;

/// <summary>
/// Un administrador (asignacion.gestionar) le quita el carro a un empleado para que otro lo pueda usar:
/// - El motivo es obligatorio (queda en la reserva y en la bitácora del carro).
/// - No se puede con un viaje en curso o en pausa: primero se finaliza el viaje.
/// - La bitácora queda a nombre del administrador (quién lo desvinculó), no del empleado.
/// - El empleado recibe una notificación push (abre "Mis solicitudes").
/// </summary>
public sealed record DesvincularAsignacionCommand(Guid SolicitudReservaId, string Motivo) : ICommand<SolicitudReservaDto>;

public sealed class DesvincularAsignacionCommandHandler : ICommandHandler<DesvincularAsignacionCommand, SolicitudReservaDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IDateTimeProvider _dateTime;
    private readonly IUsuarioActual _usuarioActual;
    private readonly NotificadorPush _notificador;
    private readonly IFileStorageService _storage;

    public DesvincularAsignacionCommandHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IDateTimeProvider dateTime,
        IUsuarioActual usuarioActual,
        NotificadorPush notificador,
        IFileStorageService storage)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _dateTime = dateTime;
        _usuarioActual = usuarioActual;
        _notificador = notificador;
        _storage = storage;
    }

    public async Task<SolicitudReservaDto> Handle(DesvincularAsignacionCommand request, CancellationToken cancellationToken)
    {
        var motivo = request.Motivo?.Trim();
        if (string.IsNullOrEmpty(motivo))
            throw new BadRequestException("El motivo para desvincular el carro es obligatorio.");

        if (motivo.Length > CambioEstatusCarro.MotivoMaxLength)
            throw new BadRequestException($"El motivo no puede exceder {CambioEstatusCarro.MotivoMaxLength} caracteres.");

        var solicitud = await _unitOfWork.SolicitudesReserva.GetByIdAsync(request.SolicitudReservaId, cancellationToken)
            ?? throw new NotFoundException($"No existe una solicitud de reserva con el id {request.SolicitudReservaId}.");

        await _usuarioActual.ValidarEmpresaAsync(solicitud.EmpresaId, cancellationToken);

        if (!solicitud.EstaAprobada)
            throw new ConflictException("El carro ya no está asignado a este empleado.");

        // Un admin de plataforma puede no tener empleado: la bitácora queda a nombre del solicitante
        var responsableId = _usuarioActual.EmpleadoId ?? solicitud.EmpleadoSolicitanteId;
        var fecha = _dateTime.Now;

        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            var cambio = await LiberacionReserva.LiberarAsync(
                _unitOfWork,
                solicitud,
                responsableId,
                fecha,
                observaciones: $"Desvinculado por un administrador. Motivo: {motivo}",
                motivoBitacora: $"Desvinculado del empleado. Motivo: {motivo}",
                cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            if (solicitud.EmpleadoSolicitanteId != _usuarioActual.EmpleadoId)
            {
                var carro = await _unitOfWork.Carros.GetByIdAsync(solicitud.CarroId, cancellationToken);
                var nombreCarro = carro is null ? "asignado" : $"{carro.Marca} {carro.Modelo} ({carro.Placas})";

                // ResultadoSolicitud: en la app abre "Mis solicitudes", donde ya aparece finalizada
                await _notificador.AgregarAsync(Notificacion.Crear(
                    solicitud.EmpresaId,
                    solicitud.EmpleadoSolicitanteId,
                    TipoNotificacion.ResultadoSolicitud,
                    solicitud.SolicitudReservaId,
                    $"Se te desvinculó del carro {nombreCarro}. Motivo: {motivo}"), cancellationToken);
            }

            await _unitOfWork.CommitTransactionAsync(cancellationToken);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }

        return await _mapper.Map<SolicitudReservaDto>(solicitud).ConDatosAsync(_unitOfWork, _storage, cancellationToken);
    }
}
