using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Application.Common.Messaging;
using APIGasoline.Application.Features.Divisiones.DTOs;
using APIGasoline.Domain.Interfaces;
using AutoMapper;

namespace APIGasoline.Application.Features.Divisiones.Commands;

/// <summary>La empresa de la división no cambia.</summary>
public sealed record UpdateDivisionCommand(Guid DivisionId, string Nombre, bool Activo) : ICommand<DivisionDto>;

public sealed class UpdateDivisionCommandHandler : ICommandHandler<UpdateDivisionCommand, DivisionDto>
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
        division.CambiarNombre(request.Nombre);

        if (request.Activo)
            division.Activar();
        else
            division.Desactivar();

        if (await _unitOfWork.Divisiones.NombreExistsAsync(division.EmpresaId, division.Nombre, division.DivisionId, cancellationToken))
            throw new ConflictException($"La empresa {division.EmpresaId} ya tiene otra división llamada '{division.Nombre}'.");

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<DivisionDto>(division);
    }
}
