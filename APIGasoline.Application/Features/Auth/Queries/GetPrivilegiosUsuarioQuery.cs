using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Application.Features.Auth.Common;
using APIGasoline.Application.Features.Auth.DTOs;
using APIGasoline.Domain.Interfaces;
using MediatR;

namespace APIGasoline.Application.Features.Auth.Queries;

/// <summary>
/// Privilegios efectivos del usuario autenticado (el id viene del token).
/// Es más ligero que el perfil: no arma el empleado ni genera URLs de fotos.
/// </summary>
public record GetPrivilegiosUsuarioQuery(Guid UsuarioId) : IRequest<PrivilegiosUsuarioDto>;

public class GetPrivilegiosUsuarioQueryHandler : IRequestHandler<GetPrivilegiosUsuarioQuery, PrivilegiosUsuarioDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetPrivilegiosUsuarioQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PrivilegiosUsuarioDto> Handle(GetPrivilegiosUsuarioQuery request, CancellationToken cancellationToken)
    {
        var usuario = await _unitOfWork.Usuarios.GetByIdAsync(request.UsuarioId, cancellationToken)
            ?? throw new UnauthorizedException("El usuario del token ya no existe.");

        var acceso = await PerfilUsuarioBuilder.CargarAccesoAsync(_unitOfWork, usuario, cancellationToken);

        return new PrivilegiosUsuarioDto(acceso.Privilegios, acceso.VersionPrivilegios);
    }
}
