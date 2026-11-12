using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Application.Common.Security;
using APIGasoline.Application.Common.Interfaces;
using APIGasoline.Application.Features.RolPrivilegios.DTOs;
using APIGasoline.Domain.Entities;
using APIGasoline.Domain.Interfaces;
using AutoMapper;
using APIGasoline.Application.Common.Messaging;

namespace APIGasoline.Application.Features.RolPrivilegios.Commands;

public sealed record CreateRolPrivilegioCommand(Guid RolId, Guid PrivilegioId) : ICommand<RolPrivilegioDto>;

public sealed class CreateRolPrivilegioCommandHandler : ICommandHandler<CreateRolPrivilegioCommand, RolPrivilegioDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    private readonly IUsuarioActual _usuarioActual;

    public CreateRolPrivilegioCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, IUsuarioActual usuarioActual)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _usuarioActual = usuarioActual;
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

        // Techo de privilegios: solo das lo que tienes
        await TechoPrivilegios.ExigirPrivilegioAsync(_usuarioActual, privilegio.Clave, cancellationToken);

        if (await _unitOfWork.RolPrivilegios.AsignacionExistsAsync(rolPrivilegio.RolId, rolPrivilegio.PrivilegioId, null, cancellationToken))
            throw new ConflictException($"El rol {rolPrivilegio.RolId} ya tiene asignado el privilegio {rolPrivilegio.PrivilegioId}.");

        await _unitOfWork.RolPrivilegios.AddAsync(rolPrivilegio, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<RolPrivilegioDto>(rolPrivilegio);
    }
}
