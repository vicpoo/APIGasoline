using APIGasoline.Application.Common.Interfaces;
using APIGasoline.Application.Common.Storage;
using APIGasoline.Application.Features.CambiosEstatusCarro.DTOs;
using APIGasoline.Domain.Interfaces;
using AutoMapper;
using MediatR;

namespace APIGasoline.Application.Features.CambiosEstatusCarro.Queries;

/// <summary>
/// Historial del más reciente al más antiguo. Filtros opcionales: empresa, carro y fechas.
/// </summary>
public record GetCambiosEstatusCarroQuery(
    Guid? EmpresaId = null,
    Guid? CarroId = null,
    DateTime? Desde = null,
    DateTime? Hasta = null) : IRequest<IReadOnlyList<CambioEstatusCarroDto>>;

public class GetCambiosEstatusCarroQueryHandler : IRequestHandler<GetCambiosEstatusCarroQuery, IReadOnlyList<CambioEstatusCarroDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IFileStorageService _storage;

    public GetCambiosEstatusCarroQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, IFileStorageService storage)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _storage = storage;
    }

    public async Task<IReadOnlyList<CambioEstatusCarroDto>> Handle(GetCambiosEstatusCarroQuery request, CancellationToken cancellationToken)
    {
        var cambios = await _unitOfWork.CambiosEstatusCarro.GetAllAsync(
            request.EmpresaId, request.CarroId, request.Desde, request.Hasta, cancellationToken);

        return await _mapper.Map<IReadOnlyList<CambioEstatusCarroDto>>(cambios).ConUrlsAsync(_storage, cancellationToken);
    }
}
