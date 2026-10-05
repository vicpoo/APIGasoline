using APIGasoline.Application.Features.Corporativos.DTOs;
using APIGasoline.Domain.Interfaces;
using AutoMapper;
using MediatR;

namespace APIGasoline.Application.Features.Corporativos.Queries;

public record GetCorporativosQuery : IRequest<IReadOnlyList<CorporativoDto>>;

public class GetCorporativosQueryHandler : IRequestHandler<GetCorporativosQuery, IReadOnlyList<CorporativoDto>>
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
        var corporativos = await _unitOfWork.Corporativos.GetAllAsync(cancellationToken);
        return _mapper.Map<IReadOnlyList<CorporativoDto>>(corporativos);
    }
}
