using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Application.Common.Interfaces;
using APIGasoline.Application.Features.Auditoria.Common;
using APIGasoline.Application.Features.Auditoria.DTOs;
using APIGasoline.Domain.Entities;
using APIGasoline.Domain.Interfaces;
using AutoMapper;
using MediatR;

namespace APIGasoline.Application.Features.Auditoria.Commands;

/// <summary>
/// Abre una revisión de auditoría. Alcance: un viaje (<see cref="ViajeId"/>),
/// o las cargas de un carro en un periodo (<see cref="CarroId"/> + periodo).
/// El revisor es el empleado del token, nunca el que mande el cliente.
/// Revisar NO es aprobar: la aprobación operativa es la de la solicitud de reserva.
/// </summary>
public record CrearRevisionAuditoriaCommand(
    Guid? ViajeId,
    Guid? CarroId,
    DateOnly? PeriodoDesde,
    DateOnly? PeriodoHasta,
    string? Observaciones) : IRequest<RevisionAuditoriaDto>;

public class CrearRevisionAuditoriaCommandHandler : IRequestHandler<CrearRevisionAuditoriaCommand, RevisionAuditoriaDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IUsuarioActual _usuarioActual;

    public CrearRevisionAuditoriaCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, IUsuarioActual usuarioActual)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _usuarioActual = usuarioActual;
    }

    public async Task<RevisionAuditoriaDto> Handle(CrearRevisionAuditoriaCommand request, CancellationToken cancellationToken)
    {
        // Valida el perfil (el revisor debe ser un empleado con acceso al módulo)
        PermisosAuditoria.ResolverEmpresa(_usuarioActual, null);

        var revisorId = _usuarioActual.EmpleadoId
            ?? throw new ForbiddenException("Solo un empleado puede registrar revisiones de auditoría.");

        var revisor = await _unitOfWork.Empleados.GetByIdAsync(revisorId, cancellationToken)
            ?? throw new NotFoundException($"No existe un empleado con el id {revisorId}.");

        RevisionAuditoria revision;

        if (request.ViajeId is not null)
        {
            if (request.CarroId is not null || request.PeriodoDesde is not null || request.PeriodoHasta is not null)
                throw new BadRequestException("Indica un viaje, o un carro con su periodo, pero no ambos.");

            var viaje = await _unitOfWork.Viajes.GetByIdAsync(request.ViajeId.Value, cancellationToken)
                ?? throw new NotFoundException($"No existe un viaje con el id {request.ViajeId}.");

            ValidarMismaEmpresa(revisor, viaje.EmpresaId);

            revision = RevisionAuditoria.DeViaje(viaje.EmpresaId, viaje.ViajeId, revisor.EmpleadoId, request.Observaciones);
        }
        else if (request.CarroId is not null)
        {
            if (request.PeriodoDesde is null || request.PeriodoHasta is null)
                throw new BadRequestException("Para revisar un carro indica el periodo (desde y hasta).");

            var carro = await _unitOfWork.Carros.GetByIdAsync(request.CarroId.Value, cancellationToken)
                ?? throw new NotFoundException($"No existe un carro con el id {request.CarroId}.");

            ValidarMismaEmpresa(revisor, carro.EmpresaId);

            revision = RevisionAuditoria.DeCarroEnPeriodo(
                carro.EmpresaId,
                carro.CarroId,
                request.PeriodoDesde.Value,
                request.PeriodoHasta.Value,
                revisor.EmpleadoId,
                request.Observaciones);
        }
        else
        {
            throw new BadRequestException("Indica qué se revisa: un viaje, o un carro con su periodo.");
        }

        await _unitOfWork.RevisionesAuditoria.AddAsync(revision, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<RevisionAuditoriaDto>(revision);
    }

    private static void ValidarMismaEmpresa(Empleado revisor, Guid empresaId)
    {
        if (revisor.EmpresaId != empresaId)
            throw new ForbiddenException("No puedes revisar registros de otra empresa.");
    }
}
