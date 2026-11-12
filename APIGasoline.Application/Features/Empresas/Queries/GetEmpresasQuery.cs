using APIGasoline.Application.Features.Empresas.DTOs;
using APIGasoline.Domain.Interfaces;
using AutoMapper;
using APIGasoline.Application.Common.Messaging;

namespace APIGasoline.Application.Features.Empresas.Queries;

/// <summary>
/// Lista las empresas no eliminadas que puede ver el usuario.
/// </summary>
public sealed record GetEmpresasQuery : IQuery<IReadOnlyList<EmpresaDto>>;

public sealed class GetEmpresasQueryHandler : IQueryHandler<GetEmpresasQuery, IReadOnlyList<EmpresaDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetEmpresasQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<EmpresaDto>> Handle(GetEmpresasQuery request, CancellationToken cancellationToken)
    {
        var empresas = await _unitOfWork.Empresas.GetAllAsync(cancellationToken);
        return _mapper.Map<IReadOnlyList<EmpresaDto>>(empresas);
    }
}
