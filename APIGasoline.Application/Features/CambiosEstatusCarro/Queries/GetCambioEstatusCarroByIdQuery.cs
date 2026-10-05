using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Application.Features.CambiosEstatusCarro.DTOs;
using APIGasoline.Domain.Interfaces;
using AutoMapper;
using MediatR;

namespace APIGasoline.Application.Features.CambiosEstatusCarro.Queries;

public record GetCambioEstatusCarroByIdQuery(int CambioEstatusCarroId) : IRequest<CambioEstatusCarroDto>;

public class GetCambioEstatusCarroByIdQueryHandler : IRequestHandler<GetCambioEstatusCarroByIdQuery, CambioEstatusCarroDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetCambioEstatusCarroByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<CambioEstatusCarroDto> Handle(GetCambioEstatusCarroByIdQuery request, CancellationToken cancellationToken)
    {
        var cambio = await _unitOfWork.CambiosEstatusCarro.GetByIdAsync(request.CambioEstatusCarroId, cancellationToken)
            ?? throw new NotFoundException($"No existe un cambio de estatus con el id {request.CambioEstatusCarroId}.");

        return _mapper.Map<CambioEstatusCarroDto>(cambio);
    }
}
