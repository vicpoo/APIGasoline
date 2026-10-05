using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Application.Features.Privilegios.DTOs;
using APIGasoline.Domain.Entities;
using APIGasoline.Domain.Interfaces;
using AutoMapper;
using MediatR;

namespace APIGasoline.Application.Features.Privilegios.Commands;

public record CreatePrivilegioCommand(string Nombre, string? Descripcion, bool Activo) : IRequest<PrivilegioDto>;

public class CreatePrivilegioCommandHandler : IRequestHandler<CreatePrivilegioCommand, PrivilegioDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreatePrivilegioCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PrivilegioDto> Handle(CreatePrivilegioCommand request, CancellationToken cancellationToken)
    {
        // El dominio valida y normaliza los datos
        var privilegio = Privilegio.Crear(request.Nombre, request.Descripcion, request.Activo);

        if (await _unitOfWork.Privilegios.NombreExistsAsync(privilegio.Nombre, null, cancellationToken))
            throw new ConflictException($"Ya existe un privilegio con el nombre '{privilegio.Nombre}'.");

        await _unitOfWork.Privilegios.AddAsync(privilegio, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // PrivilegioId ya viene asignado por la base de datos (autoincremento)
        return _mapper.Map<PrivilegioDto>(privilegio);
    }
}
