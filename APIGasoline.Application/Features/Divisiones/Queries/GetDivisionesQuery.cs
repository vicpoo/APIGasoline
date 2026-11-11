using APIGasoline.Application.Features.Divisiones.DTOs;
using APIGasoline.Domain.Interfaces;
using AutoMapper;
using APIGasoline.Application.Common.Messaging;

namespace APIGasoline.Application.Features.Divisiones.Queries;

/// <summary>
/// Lista las divisiones. Se puede filtrar por corporativo.
/// </summary>
public sealed record GetDivisionesQuery(Guid? CorporativoId = null) : IQuery<IReadOnlyList<DivisionDto>>;

public sealed class GetDivisionesQueryHandler : IQueryHandler<GetDivisionesQuery, IReadOnlyList<DivisionDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetDivisionesQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<DivisionDto>> Handle(GetDivisionesQuery request, CancellationToken cancellationToken)
    {
        var divisiones = await _unitOfWork.Divisiones.GetAllAsync(request.CorporativoId, cancellationToken);
        return _mapper.Map<IReadOnlyList<DivisionDto>>(divisiones);
    }
}
