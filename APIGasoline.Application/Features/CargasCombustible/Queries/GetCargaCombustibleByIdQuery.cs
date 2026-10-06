using APIGasoline.Application.Common.Interfaces;
using APIGasoline.Application.Common.Storage;
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
    private readonly IFileStorageService _storage;

    public GetCargaCombustibleByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, IFileStorageService storage)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _storage = storage;
    }

    public async Task<CargaCombustibleDto> Handle(GetCargaCombustibleByIdQuery request, CancellationToken cancellationToken)
    {
        var carga = await _unitOfWork.CargasCombustible.GetByIdAsync(request.CargaCombustibleId, cancellationToken)
            ?? throw new NotFoundException($"No existe una carga de combustible con el id {request.CargaCombustibleId}.");

        return await _mapper.Map<CargaCombustibleDto>(carga).ConUrlsAsync(_storage, cancellationToken);
    }
}
