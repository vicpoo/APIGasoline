using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Application.Features.CentrosCosto.DTOs;
using APIGasoline.Domain.Interfaces;
using AutoMapper;
using APIGasoline.Application.Common.Messaging;

namespace APIGasoline.Application.Features.CentrosCosto.Queries;

public sealed record GetCentroCostoByIdQuery(Guid CentroCostoId) : IQuery<CentroCostoDto>;

public sealed class GetCentroCostoByIdQueryHandler : IQueryHandler<GetCentroCostoByIdQuery, CentroCostoDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetCentroCostoByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<CentroCostoDto> Handle(GetCentroCostoByIdQuery request, CancellationToken cancellationToken)
    {
        var centroCosto = await _unitOfWork.CentrosCosto.GetByIdAsync(request.CentroCostoId, cancellationToken)
            ?? throw new NotFoundException($"No existe un centro de costo con el id {request.CentroCostoId}.");

        return _mapper.Map<CentroCostoDto>(centroCosto);
    }
}
