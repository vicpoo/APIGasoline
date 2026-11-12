using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Application.Common.Messaging;
using APIGasoline.Application.Features.Corporativos.DTOs;
using APIGasoline.Domain.Interfaces;
using AutoMapper;

namespace APIGasoline.Application.Features.Divisiones.Queries;

/// <summary>Corporativos ligados a una división (404 si la división no existe o es de otra empresa).</summary>
public sealed record GetCorporativosDeDivisionQuery(Guid DivisionId) : IQuery<IReadOnlyList<CorporativoDto>>;

public sealed class GetCorporativosDeDivisionQueryHandler : IQueryHandler<GetCorporativosDeDivisionQuery, IReadOnlyList<CorporativoDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetCorporativosDeDivisionQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<CorporativoDto>> Handle(GetCorporativosDeDivisionQuery request, CancellationToken cancellationToken)
    {
        if (!await _unitOfWork.Divisiones.ExistsAsync(request.DivisionId, cancellationToken))
            throw new NotFoundException($"No existe una división con el id {request.DivisionId}.");

        var corporativos = await _unitOfWork.Corporativos.GetAllAsync(null, request.DivisionId, cancellationToken);
        return _mapper.Map<IReadOnlyList<CorporativoDto>>(corporativos);
    }
}
