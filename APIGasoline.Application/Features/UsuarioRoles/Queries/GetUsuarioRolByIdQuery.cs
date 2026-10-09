using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Application.Features.UsuarioRoles.DTOs;
using APIGasoline.Domain.Interfaces;
using AutoMapper;
using MediatR;

namespace APIGasoline.Application.Features.UsuarioRoles.Queries;

public record GetUsuarioRolByIdQuery(Guid UsuarioRolId) : IRequest<UsuarioRolDto>;

public class GetUsuarioRolByIdQueryHandler : IRequestHandler<GetUsuarioRolByIdQuery, UsuarioRolDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetUsuarioRolByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<UsuarioRolDto> Handle(GetUsuarioRolByIdQuery request, CancellationToken cancellationToken)
    {
        var usuarioRol = await _unitOfWork.UsuarioRoles.GetByIdAsync(request.UsuarioRolId, cancellationToken)
            ?? throw new NotFoundException($"No existe una asignación usuario-rol con el id {request.UsuarioRolId}.");

        return _mapper.Map<UsuarioRolDto>(usuarioRol);
    }
}
