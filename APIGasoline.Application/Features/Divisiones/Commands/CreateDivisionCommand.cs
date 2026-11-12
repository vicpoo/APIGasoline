using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Application.Common.Messaging;
using APIGasoline.Application.Features.Divisiones.DTOs;
using APIGasoline.Domain.Entities;
using APIGasoline.Domain.Interfaces;
using AutoMapper;

namespace APIGasoline.Application.Features.Divisiones.Commands;

/// <summary>La división pertenece a una empresa; se liga a corporativos con LigarDivisionCommand.</summary>
public sealed record CreateDivisionCommand(Guid EmpresaId, string Nombre, bool Activo) : ICommand<DivisionDto>;

public sealed class CreateDivisionCommandHandler : ICommandHandler<CreateDivisionCommand, DivisionDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateDivisionCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<DivisionDto> Handle(CreateDivisionCommand request, CancellationToken cancellationToken)
    {
        // El dominio valida y normaliza los datos
        var division = Division.Crear(request.EmpresaId, request.Nombre, request.Activo);

        if (!await _unitOfWork.Empresas.ExistsAsync(division.EmpresaId, cancellationToken))
            throw new NotFoundException($"No existe una empresa con el id {division.EmpresaId}.");

        if (await _unitOfWork.Divisiones.NombreExistsAsync(division.EmpresaId, division.Nombre, null, cancellationToken))
            throw new ConflictException($"La empresa {division.EmpresaId} ya tiene una división llamada '{division.Nombre}'.");

        await _unitOfWork.Divisiones.AddAsync(division, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<DivisionDto>(division);
    }
}
