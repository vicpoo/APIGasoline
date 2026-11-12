using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Application.Common.Messaging;
using APIGasoline.Application.Features.Corporativos.DTOs;
using APIGasoline.Domain.Entities;
using APIGasoline.Domain.Interfaces;
using AutoMapper;

namespace APIGasoline.Application.Features.Corporativos.Commands;

/// <summary>Liga una división con un corporativo (N:M). Los dos deben ser de la misma empresa.</summary>
public sealed record LigarDivisionCommand(Guid CorporativoId, Guid DivisionId) : ICommand<CorporativoDivisionDto>;

public sealed class LigarDivisionCommandHandler : ICommandHandler<LigarDivisionCommand, CorporativoDivisionDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public LigarDivisionCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<CorporativoDivisionDto> Handle(LigarDivisionCommand request, CancellationToken cancellationToken)
    {
        var corporativo = await _unitOfWork.Corporativos.GetByIdAsync(request.CorporativoId, cancellationToken)
            ?? throw new NotFoundException($"No existe un corporativo con el id {request.CorporativoId}.");

        var division = await _unitOfWork.Divisiones.GetByIdAsync(request.DivisionId, cancellationToken)
            ?? throw new NotFoundException($"No existe una división con el id {request.DivisionId}.");

        if (corporativo.EmpresaId != division.EmpresaId)
            throw new BadRequestException("El corporativo y la división deben ser de la misma empresa.");

        if (await _unitOfWork.CorporativoDivisiones.ExistsAsync(corporativo.CorporativoId, division.DivisionId, cancellationToken))
            throw new ConflictException("La división ya está ligada a ese corporativo.");

        var liga = CorporativoDivision.Ligar(corporativo, division);

        await _unitOfWork.CorporativoDivisiones.AddAsync(liga, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<CorporativoDivisionDto>(liga);
    }
}
