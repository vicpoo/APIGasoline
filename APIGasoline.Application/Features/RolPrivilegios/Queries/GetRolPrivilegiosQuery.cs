using APIGasoline.Application.Features.RolPrivilegios.DTOs;
using APIGasoline.Domain.Interfaces;
using AutoMapper;
using MediatR;

namespace APIGasoline.Application.Features.RolPrivilegios.Queries;

/// <summary>
/// Lista las asignaciones. Se puede filtrar por rol, por privilegio o por ambos.
/// </summary>
public record GetRolPrivilegiosQuery(Guid? RolId = null, Guid? PrivilegioId = null) : IRequest<IReadOnlyList<RolPrivilegioDto>>;

public class GetRolPrivilegiosQueryHandler : IRequestHandler<GetRolPrivilegiosQuery, IReadOnlyList<RolPrivilegioDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetRolPrivilegiosQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<RolPrivilegioDto>> Handle(GetRolPrivilegiosQuery request, CancellationToken cancellationToken)
    {
        var rolPrivilegios = await _unitOfWork.RolPrivilegios.GetAllAsync(request.RolId, request.PrivilegioId, cancellationToken);
        return _mapper.Map<IReadOnlyList<RolPrivilegioDto>>(rolPrivilegios);
    }
}
