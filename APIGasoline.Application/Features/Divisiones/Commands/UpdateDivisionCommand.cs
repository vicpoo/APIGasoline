using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Application.Features.Divisiones.DTOs;
using APIGasoline.Domain.Interfaces;
using AutoMapper;
using MediatR;

namespace APIGasoline.Application.Features.Divisiones.Commands;

public record UpdateDivisionCommand(Guid DivisionId, Guid CorporativoId, string Nombre, bool Activo) : IRequest<DivisionDto>;

public class UpdateDivisionCommandHandler : IRequestHandler<UpdateDivisionCommand, DivisionDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UpdateDivisionCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<DivisionDto> Handle(UpdateDivisionCommand request, CancellationToken cancellationToken)
    {
        var division = await _unitOfWork.Divisiones.GetByIdAsync(request.DivisionId, cancellationToken)
            ?? throw new NotFoundException($"No existe una división con el id {request.DivisionId}.");

        // El dominio valida y normaliza los datos
        division.CambiarCorporativo(request.CorporativoId);
        division.CambiarNombre(request.Nombre);

        if (request.Activo)
            division.Activar();
        else
            division.Desactivar();

        if (!await _unitOfWork.Corporativos.ExistsAsync(division.CorporativoId, cancellationToken))
            throw new NotFoundException($"No existe un corporativo con el id {division.CorporativoId}.");

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<DivisionDto>(division);
    }
}
