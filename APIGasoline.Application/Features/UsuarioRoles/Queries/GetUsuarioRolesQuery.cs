using APIGasoline.Application.Features.UsuarioRoles.DTOs;
using APIGasoline.Domain.Interfaces;
using AutoMapper;
using APIGasoline.Application.Common.Messaging;

namespace APIGasoline.Application.Features.UsuarioRoles.Queries;

/// <summary>
/// Lista las asignaciones. Se puede filtrar por usuario, por rol o por ambos.
/// </summary>
public sealed record GetUsuarioRolesQuery(Guid? UsuarioId = null, Guid? RolId = null) : IQuery<IReadOnlyList<UsuarioRolDto>>;

public sealed class GetUsuarioRolesQueryHandler : IQueryHandler<GetUsuarioRolesQuery, IReadOnlyList<UsuarioRolDto>>
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
