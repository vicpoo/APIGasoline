using APIGasoline.Application.Features.Carros.DTOs;
using APIGasoline.Domain.Enums;
using APIGasoline.Domain.Interfaces;
using AutoMapper;
using MediatR;

namespace APIGasoline.Application.Features.Carros.Queries;

/// <summary>
/// Lista los carros. Se puede filtrar por empresa, por estado o por ambos.
/// </summary>
public record GetCarrosQuery(int? EmpresaId = null, EstadoCarro? Estado = null) : IRequest<IReadOnlyList<CarroDto>>;

public class GetCarrosQueryHandler : IRequestHandler<GetCarrosQuery, IReadOnlyList<CarroDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetCarrosQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<CarroDto>> Handle(GetCarrosQuery request, CancellationToken cancellationToken)
    {
        var carros = await _unitOfWork.Carros.GetAllAsync(request.EmpresaId, request.Estado, cancellationToken);
        return _mapper.Map<IReadOnlyList<CarroDto>>(carros);
    }
}
