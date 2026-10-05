using APIGasoline.Application.Features.CentrosCosto.DTOs;
using APIGasoline.Domain.Interfaces;
using AutoMapper;
using MediatR;

namespace APIGasoline.Application.Features.CentrosCosto.Queries;

/// <summary>
/// Lista los centros de costo. Se puede filtrar por empresa.
/// </summary>
public record GetCentrosCostoQuery(int? EmpresaId = null) : IRequest<IReadOnlyList<CentroCostoDto>>;

public class GetCentrosCostoQueryHandler : IRequestHandler<GetCentrosCostoQuery, IReadOnlyList<CentroCostoDto>>
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
        var centrosCosto = await _unitOfWork.CentrosCosto.GetAllAsync(request.EmpresaId, cancellationToken);
        return _mapper.Map<IReadOnlyList<CentroCostoDto>>(centrosCosto);
    }
}
