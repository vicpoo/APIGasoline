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

    public GetCambiosEstatusCarroQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<CambioEstatusCarroDto>> Handle(GetCambiosEstatusCarroQuery request, CancellationToken cancellationToken)
    {
        var cambios = await _unitOfWork.CambiosEstatusCarro.GetAllAsync(
            request.EmpresaId, request.CarroId, request.Desde, request.Hasta, cancellationToken);

        return _mapper.Map<IReadOnlyList<CambioEstatusCarroDto>>(cambios);
    }
}
