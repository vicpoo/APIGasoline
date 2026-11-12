using APIGasoline.Application.Common.Messaging;
using APIGasoline.Application.Features.Divisiones.DTOs;
using APIGasoline.Domain.Interfaces;
using AutoMapper;

namespace APIGasoline.Application.Features.Divisiones.Queries;

/// <summary>
/// Lista las divisiones que puede ver el usuario. Se puede filtrar por empresa
/// y por corporativo (las divisiones ligadas a él).
/// </summary>
public sealed record GetDivisionesQuery(Guid? EmpresaId = null, Guid? CorporativoId = null) : IQuery<IReadOnlyList<DivisionDto>>;

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
        var divisiones = await _unitOfWork.Divisiones.GetAllAsync(request.EmpresaId, request.CorporativoId, cancellationToken);
        return _mapper.Map<IReadOnlyList<DivisionDto>>(divisiones);
    }
}
