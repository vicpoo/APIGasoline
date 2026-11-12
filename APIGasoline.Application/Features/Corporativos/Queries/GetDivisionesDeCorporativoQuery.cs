using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Application.Common.Messaging;
using APIGasoline.Application.Features.Divisiones.DTOs;
using APIGasoline.Domain.Interfaces;
using AutoMapper;

namespace APIGasoline.Application.Features.Corporativos.Queries;

/// <summary>Divisiones ligadas a un corporativo (404 si el corporativo no existe o es de otra empresa).</summary>
public sealed record GetDivisionesDeCorporativoQuery(Guid CorporativoId) : IQuery<IReadOnlyList<DivisionDto>>;

public sealed class GetDivisionesDeCorporativoQueryHandler : IQueryHandler<GetDivisionesDeCorporativoQuery, IReadOnlyList<DivisionDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetDivisionesDeCorporativoQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<DivisionDto>> Handle(GetDivisionesDeCorporativoQuery request, CancellationToken cancellationToken)
    {
        if (!await _unitOfWork.Corporativos.ExistsAsync(request.CorporativoId, cancellationToken))
            throw new NotFoundException($"No existe un corporativo con el id {request.CorporativoId}.");

        var divisiones = await _unitOfWork.Divisiones.GetAllAsync(null, request.CorporativoId, cancellationToken);
        return _mapper.Map<IReadOnlyList<DivisionDto>>(divisiones);
    }
}
