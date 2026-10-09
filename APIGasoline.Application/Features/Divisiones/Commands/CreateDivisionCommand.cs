using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Application.Features.Divisiones.DTOs;
using APIGasoline.Domain.Entities;
using APIGasoline.Domain.Interfaces;
using AutoMapper;
using MediatR;

namespace APIGasoline.Application.Features.Divisiones.Commands;

public record CreateDivisionCommand(Guid CorporativoId, string Nombre, bool Activo) : IRequest<DivisionDto>;

public class CreateDivisionCommandHandler : IRequestHandler<CreateDivisionCommand, DivisionDto>
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
        var division = Division.Crear(request.CorporativoId, request.Nombre, request.Activo);

        if (!await _unitOfWork.Corporativos.ExistsAsync(division.CorporativoId, cancellationToken))
            throw new NotFoundException($"No existe un corporativo con el id {division.CorporativoId}.");

        await _unitOfWork.Divisiones.AddAsync(division, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // DivisionId y FechaRegistro ya vienen asignados por la base de datos
        return _mapper.Map<DivisionDto>(division);
    }
}
