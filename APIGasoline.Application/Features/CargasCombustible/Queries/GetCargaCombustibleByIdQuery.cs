using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Application.Features.CargasCombustible.DTOs;
using APIGasoline.Domain.Interfaces;
using AutoMapper;
using MediatR;

namespace APIGasoline.Application.Features.CargasCombustible.Queries;

public record GetCargaCombustibleByIdQuery(int CargaCombustibleId) : IRequest<CargaCombustibleDto>;

public class GetCargaCombustibleByIdQueryHandler : IRequestHandler<GetCargaCombustibleByIdQuery, CargaCombustibleDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetCargaCombustibleByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<CargaCombustibleDto> Handle(GetCargaCombustibleByIdQuery request, CancellationToken cancellationToken)
    {
        var carga = await _unitOfWork.CargasCombustible.GetByIdAsync(request.CargaCombustibleId, cancellationToken)
            ?? throw new NotFoundException($"No existe una carga de combustible con el id {request.CargaCombustibleId}.");

        return _mapper.Map<CargaCombustibleDto>(carga);
    }
}
