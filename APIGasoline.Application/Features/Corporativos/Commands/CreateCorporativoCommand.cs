using APIGasoline.Application.Features.Corporativos.DTOs;
using APIGasoline.Domain.Entities;
using APIGasoline.Domain.Interfaces;
using AutoMapper;
using MediatR;

namespace APIGasoline.Application.Features.Corporativos.Commands;

public record CreateCorporativoCommand(string Nombre, bool Activo) : IRequest<CorporativoDto>;

public class CreateCorporativoCommandHandler : IRequestHandler<CreateCorporativoCommand, CorporativoDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateCorporativoCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<CorporativoDto> Handle(CreateCorporativoCommand request, CancellationToken cancellationToken)
    {
        // El dominio valida y normaliza los datos
        var corporativo = Corporativo.Crear(request.Nombre, request.Activo);

        await _unitOfWork.Corporativos.AddAsync(corporativo, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // CorporativoId y FechaRegistro ya vienen asignados por la base de datos
        return _mapper.Map<CorporativoDto>(corporativo);
    }
}
