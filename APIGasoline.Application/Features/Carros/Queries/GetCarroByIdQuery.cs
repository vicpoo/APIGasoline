using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Application.Common.Interfaces;
using APIGasoline.Application.Common.Storage;
using APIGasoline.Application.Features.Carros.DTOs;
using APIGasoline.Domain.Interfaces;
using AutoMapper;
using APIGasoline.Application.Common.Messaging;

namespace APIGasoline.Application.Features.Carros.Queries;

public sealed record GetCarroByIdQuery(Guid CarroId) : IQuery<CarroDto>;

public sealed class GetCarroByIdQueryHandler : IQueryHandler<GetCarroByIdQuery, CarroDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IFileStorageService _storage;

    public GetCarroByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, IFileStorageService storage)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _storage = storage;
    }

    public async Task<CarroDto> Handle(GetCarroByIdQuery request, CancellationToken cancellationToken)
    {
        var carro = await _unitOfWork.Carros.GetByIdAsync(request.CarroId, cancellationToken)
            ?? throw new NotFoundException($"No existe un carro con el id {request.CarroId}.");

        return await _mapper.Map<CarroDto>(carro).ConUrlsAsync(_storage, cancellationToken);
    }
}
