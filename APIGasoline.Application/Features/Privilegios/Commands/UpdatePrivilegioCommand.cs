using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Application.Features.Privilegios.DTOs;
using APIGasoline.Domain.Enums;
using APIGasoline.Domain.Interfaces;
using AutoMapper;
using MediatR;

namespace APIGasoline.Application.Features.Privilegios.Commands;

/// <summary>
/// La clave NO se modifica: es lo que usan la API y el front. Si una acción deja de existir,
/// el privilegio se desactiva (Activo = false) o se elimina y se crea otro con la clave nueva.
/// Modulo null = primer segmento de la clave.
/// </summary>
public record UpdatePrivilegioCommand(
    Guid PrivilegioId,
    string? Modulo,
    string Nombre,
    string? Descripcion,
    AmbitoPrivilegio Ambito,
    bool Activo) : IRequest<PrivilegioDto>;

public class UpdatePrivilegioCommandHandler : IRequestHandler<UpdatePrivilegioCommand, PrivilegioDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UpdatePrivilegioCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PrivilegioDto> Handle(UpdatePrivilegioCommand request, CancellationToken cancellationToken)
    {
        var privilegio = await _unitOfWork.Privilegios.GetByIdAsync(request.PrivilegioId, cancellationToken)
            ?? throw new NotFoundException($"No existe un privilegio con el id {request.PrivilegioId}.");

        // Pasar a plataforma dejaría asignaciones inválidas en roles de empresa
        if (request.Ambito == AmbitoPrivilegio.Plataforma
            && privilegio.Ambito != AmbitoPrivilegio.Plataforma
            && await _unitOfWork.RolPrivilegios.AsignadoARolDeEmpresaAsync(privilegio.PrivilegioId, cancellationToken))
            throw new ConflictException(
                $"El privilegio '{privilegio.Clave}' está asignado a roles de empresa; quítalo de esos roles antes de cambiarlo a ámbito plataforma.");

        // El dominio valida y normaliza los datos
        privilegio.CambiarModulo(request.Modulo);
        privilegio.CambiarNombre(request.Nombre);
        privilegio.CambiarDescripcion(request.Descripcion);
        privilegio.CambiarAmbito(request.Ambito);

        if (request.Activo)
            privilegio.Activar();
        else
            privilegio.Desactivar();

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<PrivilegioDto>(privilegio);
    }
}
