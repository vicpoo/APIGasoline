using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Application.Features.Privilegios.DTOs;
using APIGasoline.Domain.Interfaces;
using AutoMapper;
using MediatR;

namespace APIGasoline.Application.Features.Privilegios.Commands;

public record UpdatePrivilegioCommand(Guid PrivilegioId, string Nombre, string? Descripcion, bool Activo) : IRequest<PrivilegioDto>;

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

        // El dominio valida y normaliza los datos
        privilegio.CambiarNombre(request.Nombre);
        privilegio.CambiarDescripcion(request.Descripcion);

        if (request.Activo)
            privilegio.Activar();
        else
            privilegio.Desactivar();

        if (await _unitOfWork.Privilegios.NombreExistsAsync(privilegio.Nombre, privilegio.PrivilegioId, cancellationToken))
            throw new ConflictException($"Ya existe otro privilegio con el nombre '{privilegio.Nombre}'.");

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<PrivilegioDto>(privilegio);
    }
}
