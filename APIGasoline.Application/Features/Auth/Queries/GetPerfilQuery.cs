using APIGasoline.Application.Common.Interfaces;
using APIGasoline.Application.Common.Storage;
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
public record GetPerfilQuery(Guid UsuarioId) : IRequest<PerfilUsuarioDto>;

public class GetPerfilQueryHandler : IRequestHandler<GetPerfilQuery, PerfilUsuarioDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IFileStorageService _storage;

    public GetPerfilQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, IFileStorageService storage)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _storage = storage;
    }

    public async Task<PerfilUsuarioDto> Handle(GetPerfilQuery request, CancellationToken cancellationToken)
    {
        var usuario = await _unitOfWork.Usuarios.GetByIdAsync(request.UsuarioId, cancellationToken)
            ?? throw new UnauthorizedException("El usuario del token ya no existe.");

        var (perfil, _) = await PerfilUsuarioBuilder.ConstruirAsync(_unitOfWork, _mapper, _storage, usuario, cancellationToken);
        return perfil;
    }
}
