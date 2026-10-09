using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Application.Features.Corporativos.DTOs;
using APIGasoline.Domain.Interfaces;
using AutoMapper;
using MediatR;

namespace APIGasoline.Application.Features.Corporativos.Commands;

public record UpdateCorporativoCommand(Guid CorporativoId, string Nombre, bool Activo) : IRequest<CorporativoDto>;

public class UpdateCorporativoCommandHandler : IRequestHandler<UpdateCorporativoCommand, CorporativoDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UpdateCorporativoCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<CorporativoDto> Handle(UpdateCorporativoCommand request, CancellationToken cancellationToken)
    {
        var corporativo = await _unitOfWork.Corporativos.GetByIdAsync(request.CorporativoId, cancellationToken)
            ?? throw new NotFoundException($"No existe un corporativo con el id {request.CorporativoId}.");

        // El dominio valida y normaliza los datos
        corporativo.CambiarNombre(request.Nombre);

        if (request.Activo)
            corporativo.Activar();
        else
            corporativo.Desactivar();

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<CorporativoDto>(corporativo);
    }
}
