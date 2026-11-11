using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Application.Features.Corporativos.DTOs;
using APIGasoline.Domain.Interfaces;
using AutoMapper;
using APIGasoline.Application.Common.Messaging;

namespace APIGasoline.Application.Features.Corporativos.Queries;

public sealed record GetCorporativoByIdQuery(Guid CorporativoId) : IQuery<CorporativoDto>;

public sealed class GetCorporativoByIdQueryHandler : IQueryHandler<GetCorporativoByIdQuery, CorporativoDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetCorporativoByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<CorporativoDto> Handle(GetCorporativoByIdQuery request, CancellationToken cancellationToken)
    {
        var corporativo = await _unitOfWork.Corporativos.GetByIdAsync(request.CorporativoId, cancellationToken)
            ?? throw new NotFoundException($"No existe un corporativo con el id {request.CorporativoId}.");

        return _mapper.Map<CorporativoDto>(corporativo);
    }
}
