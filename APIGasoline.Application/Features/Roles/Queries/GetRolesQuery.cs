using APIGasoline.Application.Features.Roles.DTOs;
using APIGasoline.Domain.Interfaces;
using AutoMapper;
using MediatR;

namespace APIGasoline.Application.Features.Roles.Queries;

/// <summary>
/// EmpresaId → roles disponibles para esa empresa (globales + exclusivos).
/// SoloGlobales → solo los globales. Sin filtros → todos.
/// </summary>
public record GetRolesQuery(Guid? EmpresaId = null, bool SoloGlobales = false) : IRequest<IReadOnlyList<RolDto>>;

public class GetRolesQueryHandler : IRequestHandler<GetRolesQuery, IReadOnlyList<RolDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetRolesQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<RolDto>> Handle(GetRolesQuery request, CancellationToken cancellationToken)
    {
        var roles = await _unitOfWork.Roles.GetAllAsync(request.EmpresaId, request.SoloGlobales, cancellationToken);
        return _mapper.Map<IReadOnlyList<RolDto>>(roles);
    }
}
