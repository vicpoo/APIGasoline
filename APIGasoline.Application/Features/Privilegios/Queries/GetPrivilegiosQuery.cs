using APIGasoline.Application.Features.Privilegios.DTOs;
using APIGasoline.Domain.Interfaces;
using AutoMapper;
using APIGasoline.Application.Common.Messaging;

namespace APIGasoline.Application.Features.Privilegios.Queries;

/// <summary>Modulo opcional para filtrar (ej. carro).</summary>
public sealed record GetPrivilegiosQuery(string? Modulo = null) : IQuery<IReadOnlyList<PrivilegioDto>>;

public sealed class GetPrivilegiosQueryHandler : IQueryHandler<GetPrivilegiosQuery, IReadOnlyList<PrivilegioDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetPrivilegiosQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<PrivilegioDto>> Handle(GetPrivilegiosQuery request, CancellationToken cancellationToken)
    {
        var privilegios = await _unitOfWork.Privilegios.GetAllAsync(request.Modulo, cancellationToken);
        return _mapper.Map<IReadOnlyList<PrivilegioDto>>(privilegios);
    }
}
