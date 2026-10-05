using APIGasoline.Application.Features.Empresas.DTOs;
using APIGasoline.Domain.Interfaces;
using AutoMapper;
using MediatR;

namespace APIGasoline.Application.Features.Empresas.Queries;

/// <summary>
/// Lista las empresas no eliminadas. Se puede filtrar por corporativo, por división o por ambos.
/// </summary>
public record GetEmpresasQuery(int? CorporativoId = null, int? DivisionId = null) : IRequest<IReadOnlyList<EmpresaDto>>;

public class GetEmpresasQueryHandler : IRequestHandler<GetEmpresasQuery, IReadOnlyList<EmpresaDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetEmpresasQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<EmpresaDto>> Handle(GetEmpresasQuery request, CancellationToken cancellationToken)
    {
        var empresas = await _unitOfWork.Empresas.GetAllAsync(request.CorporativoId, request.DivisionId, cancellationToken);
        return _mapper.Map<IReadOnlyList<EmpresaDto>>(empresas);
    }
}
