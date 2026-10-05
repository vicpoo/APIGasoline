using APIGasoline.Application.Features.UsuarioRoles.DTOs;
using APIGasoline.Domain.Interfaces;
using AutoMapper;
using MediatR;

namespace APIGasoline.Application.Features.UsuarioRoles.Queries;

/// <summary>
/// Lista las asignaciones. Se puede filtrar por usuario, por rol o por ambos.
/// </summary>
public record GetUsuarioRolesQuery(int? UsuarioId = null, int? RolId = null) : IRequest<IReadOnlyList<UsuarioRolDto>>;

public class GetUsuarioRolesQueryHandler : IRequestHandler<GetUsuarioRolesQuery, IReadOnlyList<UsuarioRolDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetUsuarioRolesQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<UsuarioRolDto>> Handle(GetUsuarioRolesQuery request, CancellationToken cancellationToken)
    {
        var usuarioRoles = await _unitOfWork.UsuarioRoles.GetAllAsync(request.UsuarioId, request.RolId, cancellationToken);
        return _mapper.Map<IReadOnlyList<UsuarioRolDto>>(usuarioRoles);
    }
}
