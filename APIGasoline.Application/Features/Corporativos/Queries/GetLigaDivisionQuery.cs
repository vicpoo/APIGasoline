using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Application.Common.Messaging;
using APIGasoline.Application.Features.Corporativos.DTOs;
using APIGasoline.Domain.Interfaces;
using AutoMapper;

namespace APIGasoline.Application.Features.Corporativos.Queries;

/// <summary>La liga entre un corporativo y una división (404 si no están ligados o son de otra empresa).</summary>
public sealed record GetLigaDivisionQuery(Guid CorporativoId, Guid DivisionId) : IQuery<CorporativoDivisionDto>;

public sealed class GetLigaDivisionQueryHandler : IQueryHandler<GetLigaDivisionQuery, CorporativoDivisionDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetLigaDivisionQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<CorporativoDivisionDto> Handle(GetLigaDivisionQuery request, CancellationToken cancellationToken)
    {
        var liga = await _unitOfWork.CorporativoDivisiones.GetAsync(request.CorporativoId, request.DivisionId, cancellationToken)
            ?? throw new NotFoundException($"La división {request.DivisionId} no está ligada al corporativo {request.CorporativoId}.");

        return _mapper.Map<CorporativoDivisionDto>(liga);
    }
}
