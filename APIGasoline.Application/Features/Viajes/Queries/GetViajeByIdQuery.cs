using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Application.Features.Viajes.DTOs;
using APIGasoline.Domain.Interfaces;
using AutoMapper;
using MediatR;

namespace APIGasoline.Application.Features.Viajes.Queries;

public record GetViajeByIdQuery(Guid ViajeId) : IRequest<ViajeDto>;

public class GetViajeByIdQueryHandler : IRequestHandler<GetViajeByIdQuery, ViajeDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetViajeByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ViajeDto> Handle(GetViajeByIdQuery request, CancellationToken cancellationToken)
    {
        var viaje = await _unitOfWork.Viajes.GetByIdAsync(request.ViajeId, cancellationToken)
            ?? throw new NotFoundException($"No existe un viaje con el id {request.ViajeId}.");

        return _mapper.Map<ViajeDto>(viaje);
    }
}
