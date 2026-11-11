using APIGasoline.Application.Common.Interfaces;
using APIGasoline.Application.Common.Storage;
using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Application.Features.CambiosEstatusCarro.DTOs;
using APIGasoline.Domain.Interfaces;
using AutoMapper;
using APIGasoline.Application.Common.Messaging;

namespace APIGasoline.Application.Features.CambiosEstatusCarro.Queries;

public sealed record GetCambioEstatusCarroByIdQuery(Guid CambioEstatusCarroId) : IQuery<CambioEstatusCarroDto>;

public sealed class GetCambioEstatusCarroByIdQueryHandler : IQueryHandler<GetCambioEstatusCarroByIdQuery, CambioEstatusCarroDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IFileStorageService _storage;

    public GetCambioEstatusCarroByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, IFileStorageService storage)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _storage = storage;
    }

    public async Task<CambioEstatusCarroDto> Handle(GetCambioEstatusCarroByIdQuery request, CancellationToken cancellationToken)
    {
        var cambio = await _unitOfWork.CambiosEstatusCarro.GetByIdAsync(request.CambioEstatusCarroId, cancellationToken)
            ?? throw new NotFoundException($"No existe un cambio de estatus con el id {request.CambioEstatusCarroId}.");

        return await _mapper.Map<CambioEstatusCarroDto>(cambio).ConUrlsAsync(_storage, cancellationToken);
    }
}
