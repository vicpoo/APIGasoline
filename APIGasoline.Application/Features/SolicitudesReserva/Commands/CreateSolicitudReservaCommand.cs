using APIGasoline.Application.Common.Interfaces;
using APIGasoline.Application.Common.Security;
using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Application.Features.SolicitudesReserva.Common;
using APIGasoline.Application.Features.SolicitudesReserva.DTOs;
using APIGasoline.Domain.Entities;
using APIGasoline.Domain.Enums;
using APIGasoline.Domain.Interfaces;
using AutoMapper;
using MediatR;
using APIGasoline.Application.Common.Outbox;

namespace APIGasoline.Application.Features.SolicitudesReserva.Commands;

/// <param name="Motivo">v8. Para qué necesita el carro (obligatorio, máx. 255).</param>
public record CreateSolicitudReservaCommand(Guid EmpresaId, Guid CarroId, Guid EmpleadoSolicitanteId, string Motivo) : IRequest<SolicitudReservaDto>, IRecursoPropio
{
    /// <summary>Cada quien solicita para sí mismo; a nombre de otro, solo con solicitud.aprobar.</summary>
    public bool EsPropio(IUsuarioActual usuario)
        => EmpleadoSolicitanteId == usuario.EmpleadoId;

    public string PrivilegioParaOtros => Permisos.SolicitudAprobar;
}

public class CreateSolicitudReservaCommandHandler : IRequestHandler<CreateSolicitudReservaCommand, SolicitudReservaDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly NotificadorPush _notificador;
    private readonly IFileStorageService _storage;

    public CreateSolicitudReservaCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, NotificadorPush notificador, IFileStorageService storage)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _notificador = notificador;
        _storage = storage;
    }

    public async Task<SolicitudReservaDto> Handle(CreateSolicitudReservaCommand request, CancellationToken cancellationToken)
    {
        // El dominio valida los datos
        var solicitud = SolicitudReserva.Crear(request.EmpresaId, request.CarroId, request.EmpleadoSolicitanteId, request.Motivo);

        if (!await _unitOfWork.Empresas.ExistsAsync(solicitud.EmpresaId, cancellationToken))
            throw new NotFoundException($"No existe una empresa con el id {solicitud.EmpresaId}.");

        var carro = await _unitOfWork.Carros.GetByIdAsync(solicitud.CarroId, cancellationToken)
            ?? throw new NotFoundException($"No existe un carro con el id {solicitud.CarroId}.");

        if (carro.EmpresaId != solicitud.EmpresaId)
            throw new BadRequestException($"El carro {carro.CarroId} no pertenece a la empresa {solicitud.EmpresaId}.");

        if (carro.DadoDeBaja)
            throw new ConflictException($"El carro {carro.CarroId} está dado de baja.");

        if (carro.Estado == EstadoCarro.EnMantenimiento)
            throw new ConflictException($"El carro {carro.CarroId} está en mantenimiento.");

        if (carro.Estado == EstadoCarro.Ocupado)
            throw new ConflictException($"El carro {carro.CarroId} está ocupado.");

        var solicitante = await _unitOfWork.Empleados.GetByIdAsync(solicitud.EmpleadoSolicitanteId, cancellationToken)
            ?? throw new NotFoundException($"No existe un empleado con el id {solicitud.EmpleadoSolicitanteId}.");

        if (solicitante.EmpresaId != solicitud.EmpresaId)
            throw new BadRequestException($"El empleado {solicitante.EmpleadoId} no pertenece a la empresa {solicitud.EmpresaId}.");

        // Regla: el empleado debe haber subido su licencia antes de usar la app
        if (!solicitante.TieneLicencia)
            throw new BadRequestException("El empleado debe subir su licencia de conducir antes de solicitar un carro.");

        if (await _unitOfWork.SolicitudesReserva.ExistePendienteAsync(solicitud.EmpleadoSolicitanteId, solicitud.CarroId, cancellationToken))
            throw new ConflictException("El empleado ya tiene una solicitud pendiente para este carro.");

        // Unit of Work con transacción: primero se guarda la solicitud (para tener su id)
        // y luego las notificaciones que la referencian; si algo falla, no queda nada a medias.
        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            await _unitOfWork.SolicitudesReserva.AddAsync(solicitud, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var confirmadores = await _unitOfWork.Empleados.GetIdsConPrivilegioAsync(solicitud.EmpresaId, Permisos.SolicitudAprobar, cancellationToken);

            var notificaciones = confirmadores
                .Where(id => id != solicitud.EmpleadoSolicitanteId)
                .Select(id => Notificacion.Crear(
                    solicitud.EmpresaId,
                    id,
                    TipoNotificacion.NuevaSolicitud,
                    solicitud.SolicitudReservaId,
                    $"{solicitante.NombreCompleto} solicitó el carro {carro.Marca} {carro.Modelo} ({carro.Placas})."));

            await _notificador.AgregarAsync(notificaciones, cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }

        // SolicitudReservaId y FechaSolicitud ya vienen asignados por la base de datos
        return await _mapper.Map<SolicitudReservaDto>(solicitud).ConDatosAsync(_unitOfWork, _storage, cancellationToken);
    }
}
