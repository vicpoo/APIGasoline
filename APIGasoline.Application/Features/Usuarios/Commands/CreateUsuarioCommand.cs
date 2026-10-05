using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Application.Common.Interfaces;
using APIGasoline.Application.Common.Security;
using APIGasoline.Application.Features.Usuarios.DTOs;
using APIGasoline.Domain.Entities;
using APIGasoline.Domain.Enums;
using APIGasoline.Domain.Interfaces;
using AutoMapper;
using MediatR;

namespace APIGasoline.Application.Features.Usuarios.Commands;

/// <summary>
/// Crea un usuario con su contraseña (se guarda solo el hash).
/// </summary>
public record CreateUsuarioCommand(string Email, string Password, EstadoUsuario Estado) : IRequest<UsuarioDto>;

public class CreateUsuarioCommandHandler : IRequestHandler<CreateUsuarioCommand, UsuarioDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IPasswordHasher _passwordHasher;

    public CreateUsuarioCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, IPasswordHasher passwordHasher)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _passwordHasher = passwordHasher;
    }

    public async Task<UsuarioDto> Handle(CreateUsuarioCommand request, CancellationToken cancellationToken)
    {
        PasswordPolicy.Validar(request.Password);

        // El dominio valida y normaliza los datos
        var usuario = Usuario.Crear(request.Email, _passwordHasher.Hash(request.Password), request.Estado);

        if (await _unitOfWork.Usuarios.EmailExistsAsync(usuario.Email, null, cancellationToken))
            throw new ConflictException($"Ya existe un usuario con el email '{usuario.Email}'.");

        await _unitOfWork.Usuarios.AddAsync(usuario, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // UsuarioId y FechaRegistro ya vienen asignados por la base de datos
        return _mapper.Map<UsuarioDto>(usuario);
    }
}
