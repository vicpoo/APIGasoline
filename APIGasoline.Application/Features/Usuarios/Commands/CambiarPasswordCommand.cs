using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Application.Common.Interfaces;
using APIGasoline.Application.Common.Security;
using APIGasoline.Domain.Interfaces;
using MediatR;

namespace APIGasoline.Application.Features.Usuarios.Commands;

/// <summary>
/// Cambia la contraseña verificando primero la actual.
/// </summary>
public record CambiarPasswordCommand(int UsuarioId, string PasswordActual, string PasswordNueva) : IRequest;

public class CambiarPasswordCommandHandler : IRequestHandler<CambiarPasswordCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;

    public CambiarPasswordCommandHandler(IUnitOfWork unitOfWork, IPasswordHasher passwordHasher)
    {
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
    }

    public async Task Handle(CambiarPasswordCommand request, CancellationToken cancellationToken)
    {
        var usuario = await _unitOfWork.Usuarios.GetByIdAsync(request.UsuarioId, cancellationToken)
            ?? throw new NotFoundException($"No existe un usuario con el id {request.UsuarioId}.");

        if (!_passwordHasher.Verify(request.PasswordActual, usuario.PasswordHash))
            throw new UnauthorizedException("La contraseña actual es incorrecta.");

        if (request.PasswordActual == request.PasswordNueva)
            throw new BadRequestException("La contraseña nueva debe ser diferente a la actual.");

        PasswordPolicy.Validar(request.PasswordNueva);

        usuario.CambiarPasswordHash(_passwordHasher.Hash(request.PasswordNueva));
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
