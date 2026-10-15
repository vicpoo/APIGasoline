using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Application.Features.Privilegios.DTOs;
using APIGasoline.Domain.Entities;
using APIGasoline.Domain.Enums;
using APIGasoline.Domain.Interfaces;
using AutoMapper;
using MediatR;

namespace APIGasoline.Application.Features.Privilegios.Commands;

/// <summary>
/// Clave = código inmutable modulo.accion (ej. carro.crear). Modulo null = primer segmento de la clave.
/// </summary>
public record CreatePrivilegioCommand(
    string Clave,
    string? Modulo,
    string Nombre,
    string? Descripcion,
    AmbitoPrivilegio Ambito,
    bool Activo) : IRequest<PrivilegioDto>;

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
        // El dominio valida y normaliza los datos (la clave queda en minúsculas)
        var privilegio = Privilegio.Crear(
            request.Clave,
            request.Nombre,
            request.Descripcion,
            request.Ambito,
            request.Activo,
            request.Modulo);

        if (await _unitOfWork.Privilegios.ClaveExistsAsync(privilegio.Clave, cancellationToken))
            throw new ConflictException($"Ya existe un privilegio con la clave '{privilegio.Clave}'.");

        await _unitOfWork.Privilegios.AddAsync(privilegio, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<PrivilegioDto>(privilegio);
    }
}
