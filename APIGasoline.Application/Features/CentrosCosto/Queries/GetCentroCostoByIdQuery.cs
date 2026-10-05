using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Application.Features.CentrosCosto.DTOs;
using APIGasoline.Domain.Interfaces;
using AutoMapper;
using MediatR;

namespace APIGasoline.Application.Features.CentrosCosto.Queries;

public record GetCentroCostoByIdQuery(int CentroCostoId) : IRequest<CentroCostoDto>;

public class GetCentroCostoByIdQueryHandler : IRequestHandler<GetCentroCostoByIdQuery, CentroCostoDto>
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
