using APIGasoline.Application.Common.Messaging;
using APIGasoline.Application.Features.Corporativos.DTOs;
using APIGasoline.Domain.Interfaces;
using AutoMapper;

namespace APIGasoline.Application.Features.Corporativos.Queries;

/// <summary>
/// Lista los corporativos que puede ver el usuario. Se puede filtrar por empresa
/// y por división (los corporativos ligados a ella).
/// </summary>
public sealed record GetCorporativosQuery(Guid? EmpresaId = null, Guid? DivisionId = null) : IQuery<IReadOnlyList<CorporativoDto>>;

public sealed class GetCorporativosQueryHandler : IQueryHandler<GetCorporativosQuery, IReadOnlyList<CorporativoDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetCorporativosQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<CorporativoDto>> Handle(GetCorporativosQuery request, CancellationToken cancellationToken)
    {
        var corporativos = await _unitOfWork.Corporativos.GetAllAsync(request.EmpresaId, request.DivisionId, cancellationToken);
        return _mapper.Map<IReadOnlyList<CorporativoDto>>(corporativos);
    }
}
