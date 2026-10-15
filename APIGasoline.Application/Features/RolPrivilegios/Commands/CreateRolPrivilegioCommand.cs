using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Application.Features.RolPrivilegios.DTOs;
using APIGasoline.Domain.Entities;
using APIGasoline.Domain.Interfaces;
using AutoMapper;
using MediatR;

namespace APIGasoline.Application.Features.RolPrivilegios.Commands;

public record CreateRolPrivilegioCommand(Guid RolId, Guid PrivilegioId) : IRequest<RolPrivilegioDto>;

public class CreateRolPrivilegioCommandHandler : IRequestHandler<CreateRolPrivilegioCommand, RolPrivilegioDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateRolPrivilegioCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<RolPrivilegioDto> Handle(CreateRolPrivilegioCommand request, CancellationToken cancellationToken)
    {
        // El dominio valida los datos
        var rolPrivilegio = RolPrivilegio.Crear(request.RolId, request.PrivilegioId);

        var rol = await _unitOfWork.Roles.GetByIdAsync(rolPrivilegio.RolId, cancellationToken)
            ?? throw new NotFoundException($"No existe un rol con el id {rolPrivilegio.RolId}.");

        var privilegio = await _unitOfWork.Privilegios.GetByIdAsync(rolPrivilegio.PrivilegioId, cancellationToken)
            ?? throw new NotFoundException($"No existe un privilegio con el id {rolPrivilegio.PrivilegioId}.");

        // Un privilegio de plataforma (ej. empresa.crear) solo se asigna a roles globales
        if (!privilegio.AsignableA(rol))
            throw new BadRequestException(
                $"El privilegio '{privilegio.Clave}' es de plataforma y solo se puede asignar a roles globales; el rol '{rol.Nombre}' es de una empresa.");

        if (await _unitOfWork.RolPrivilegios.AsignacionExistsAsync(rolPrivilegio.RolId, rolPrivilegio.PrivilegioId, null, cancellationToken))
            throw new ConflictException($"El rol {rolPrivilegio.RolId} ya tiene asignado el privilegio {rolPrivilegio.PrivilegioId}.");

        await _unitOfWork.RolPrivilegios.AddAsync(rolPrivilegio, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<RolPrivilegioDto>(rolPrivilegio);
    }
}
