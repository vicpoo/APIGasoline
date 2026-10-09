using APIGasoline.Application.Common.Interfaces;
using APIGasoline.Application.Common.Storage;
using APIGasoline.Application.Features.Carros.DTOs;
using APIGasoline.Domain.Enums;
using APIGasoline.Domain.Interfaces;
using AutoMapper;
using MediatR;

namespace APIGasoline.Application.Features.Carros.Queries;

/// <summary>
/// Lista los carros. Se puede filtrar por empresa, por estado o por ambos.
/// </summary>
public record GetCarrosQuery(Guid? EmpresaId = null, EstadoCarro? Estado = null) : IRequest<IReadOnlyList<CarroDto>>;

public class GetCarrosQueryHandler : IRequestHandler<GetCarrosQuery, IReadOnlyList<CarroDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IFileStorageService _storage;

    public GetCarrosQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, IFileStorageService storage)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _storage = storage;
    }

    public async Task<IReadOnlyList<CarroDto>> Handle(GetCarrosQuery request, CancellationToken cancellationToken)
    {
        var carros = await _unitOfWork.Carros.GetAllAsync(request.EmpresaId, request.Estado, cancellationToken);
        return await _mapper.Map<IReadOnlyList<CarroDto>>(carros).ConUrlsAsync(_storage, cancellationToken);
    }
}
