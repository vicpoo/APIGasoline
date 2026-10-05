using APIGasoline.Application.Common.Constants;
using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Application.Features.SolicitudesReserva.DTOs;
using APIGasoline.Domain.Entities;
using APIGasoline.Domain.Enums;
using APIGasoline.Domain.Interfaces;
using AutoMapper;
using MediatR;

namespace APIGasoline.Application.Features.SolicitudesReserva.Commands;

public record CreateSolicitudReservaCommand(int EmpresaId, int CarroId, int EmpleadoSolicitanteId) : IRequest<SolicitudReservaDto>;

public class CreateSolicitudReservaCommandHandler : IRequestHandler<CreateSolicitudReservaCommand, SolicitudReservaDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateSolicitudReservaCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<SolicitudReservaDto> Handle(CreateSolicitudReservaCommand request, CancellationToken cancellationToken)
    {
        // El dominio valida los datos
        var solicitud = SolicitudReserva.Crear(request.EmpresaId, request.CarroId, request.EmpleadoSolicitanteId);

        if (!await _unitOfWork.Empresas.ExistsAsync(solicitud.EmpresaId, cancellationToken))
            throw new NotFoundException($"No existe una empresa con el id {solicitud.EmpresaId}.");

        var carro = await _unitOfWork.Carros.GetByIdAsync(solicitud.CarroId, cancellationToken)
            ?? throw new NotFoundException($"No existe un carro con el id {solicitud.CarroId}.");

        if (carro.EmpresaId != solicitud.EmpresaId)
            throw new BadRequestException($"El carro {carro.CarroId} no pertenece a la empresa {solicitud.EmpresaId}.");

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

            var confirmadores = await _unitOfWork.Empleados.GetIdsConRolAsync(solicitud.EmpresaId, NombresRol.Confirmador, cancellationToken);

            var notificaciones = confirmadores
                .Where(id => id != solicitud.EmpleadoSolicitanteId)
                .Select(id => Notificacion.Crear(
                    solicitud.EmpresaId,
                    id,
                    TipoNotificacion.NuevaSolicitud,
                    solicitud.SolicitudReservaId,
                    $"{solicitante.NombreCompleto} solicitó el carro {carro.Marca} {carro.Modelo} ({carro.Placas})."));

            await _unitOfWork.Notificaciones.AddRangeAsync(notificaciones, cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }

        // SolicitudReservaId y FechaSolicitud ya vienen asignados por la base de datos
        return _mapper.Map<SolicitudReservaDto>(solicitud);
    }
}
