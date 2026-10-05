using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Application.Features.Carros.DTOs;
using APIGasoline.Domain.Interfaces;
using AutoMapper;
using MediatR;

namespace APIGasoline.Application.Features.Carros.Queries;

public record GetCarroByIdQuery(int CarroId) : IRequest<CarroDto>;

public class GetCarroByIdQueryHandler : IRequestHandler<GetCarroByIdQuery, CarroDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetCarroByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<CarroDto> Handle(GetCarroByIdQuery request, CancellationToken cancellationToken)
    {
        var carro = await _unitOfWork.Carros.GetByIdAsync(request.CarroId, cancellationToken)
            ?? throw new NotFoundException($"No existe un carro con el id {request.CarroId}.");

        return _mapper.Map<CarroDto>(carro);
    }
}
