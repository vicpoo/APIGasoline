using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Application.Features.RolPrivilegios.DTOs;
using APIGasoline.Domain.Interfaces;
using AutoMapper;
using MediatR;

namespace APIGasoline.Application.Features.RolPrivilegios.Commands;

public record UpdateRolPrivilegioCommand(Guid RolPrivilegioId, Guid RolId, Guid PrivilegioId) : IRequest<RolPrivilegioDto>;

public class UpdateRolPrivilegioCommandHandler : IRequestHandler<UpdateRolPrivilegioCommand, RolPrivilegioDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UpdateRolPrivilegioCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<RolPrivilegioDto> Handle(UpdateRolPrivilegioCommand request, CancellationToken cancellationToken)
    {
        var rolPrivilegio = await _unitOfWork.RolPrivilegios.GetByIdAsync(request.RolPrivilegioId, cancellationToken)
            ?? throw new NotFoundException($"No existe una asignación rol-privilegio con el id {request.RolPrivilegioId}.");

        // El dominio valida los datos
        rolPrivilegio.Reasignar(request.RolId, request.PrivilegioId);

        var rol = await _unitOfWork.Roles.GetByIdAsync(rolPrivilegio.RolId, cancellationToken)
            ?? throw new NotFoundException($"No existe un rol con el id {rolPrivilegio.RolId}.");

        var privilegio = await _unitOfWork.Privilegios.GetByIdAsync(rolPrivilegio.PrivilegioId, cancellationToken)
            ?? throw new NotFoundException($"No existe un privilegio con el id {rolPrivilegio.PrivilegioId}.");

        // Un privilegio de plataforma (ej. empresa.crear) solo se asigna a roles globales
        if (!privilegio.AsignableA(rol))
            throw new BadRequestException(
                $"El privilegio '{privilegio.Clave}' es de plataforma y solo se puede asignar a roles globales; el rol '{rol.Nombre}' es de una empresa.");

        if (await _unitOfWork.RolPrivilegios.AsignacionExistsAsync(rolPrivilegio.RolId, rolPrivilegio.PrivilegioId, rolPrivilegio.RolPrivilegioId, cancellationToken))
            throw new ConflictException($"El rol {rolPrivilegio.RolId} ya tiene asignado el privilegio {rolPrivilegio.PrivilegioId}.");

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<RolPrivilegioDto>(rolPrivilegio);
    }
}
