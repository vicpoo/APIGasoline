using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Application.Features.Auth.Common;
using APIGasoline.Application.Features.Auth.DTOs;
using APIGasoline.Domain.Interfaces;
using AutoMapper;
using MediatR;

namespace APIGasoline.Application.Features.Auth.Queries;

/// <summary>
/// Perfil del usuario autenticado (el id viene del token).
/// </summary>
public record GetPerfilQuery(int UsuarioId) : IRequest<PerfilUsuarioDto>;

public class GetPerfilQueryHandler : IRequestHandler<GetPerfilQuery, PerfilUsuarioDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetPerfilQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PerfilUsuarioDto> Handle(GetPerfilQuery request, CancellationToken cancellationToken)
    {
        var usuario = await _unitOfWork.Usuarios.GetByIdAsync(request.UsuarioId, cancellationToken)
            ?? throw new UnauthorizedException("El usuario del token ya no existe.");

        var (perfil, _) = await PerfilUsuarioBuilder.ConstruirAsync(_unitOfWork, _mapper, usuario, cancellationToken);
        return perfil;
    }
}
