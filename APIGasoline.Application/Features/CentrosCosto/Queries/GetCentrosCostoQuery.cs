using APIGasoline.Application.Features.CentrosCosto.DTOs;
using APIGasoline.Domain.Interfaces;
using AutoMapper;
using APIGasoline.Application.Common.Messaging;

namespace APIGasoline.Application.Features.CentrosCosto.Queries;

/// <summary>
/// Lista los centros de costo. Se puede filtrar por empresa, corporativo y división.
/// </summary>
public sealed record GetCentrosCostoQuery(Guid? EmpresaId = null, Guid? CorporativoId = null, Guid? DivisionId = null) : IQuery<IReadOnlyList<CentroCostoDto>>;

public sealed class GetCentrosCostoQueryHandler : IQueryHandler<GetCentrosCostoQuery, IReadOnlyList<CentroCostoDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetCentrosCostoQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<CentroCostoDto>> Handle(GetCentrosCostoQuery request, CancellationToken cancellationToken)
    {
        var centrosCosto = await _unitOfWork.CentrosCosto.GetAllAsync(request.EmpresaId, request.CorporativoId, request.DivisionId, cancellationToken);
        return _mapper.Map<IReadOnlyList<CentroCostoDto>>(centrosCosto);
    }
}
