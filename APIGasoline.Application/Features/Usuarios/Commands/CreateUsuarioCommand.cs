using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Application.Common.Interfaces;
using APIGasoline.Application.Common.Security;
using APIGasoline.Application.Features.Usuarios.DTOs;
using APIGasoline.Domain.Entities;
using APIGasoline.Domain.Enums;
using APIGasoline.Domain.Interfaces;
using AutoMapper;
using APIGasoline.Application.Common.Messaging;

namespace APIGasoline.Application.Features.Usuarios.Commands;

/// <summary>
/// Crea un usuario con su contraseña (se guarda solo el hash).
/// La cuenta queda en la empresa de quien la crea; solo la plataforma elige la empresa (o ninguna).
/// </summary>
public sealed record CreateUsuarioCommand(string Email, string Password, EstadoUsuario Estado, Guid? EmpresaId = null) : ICommand<UsuarioDto>;

public sealed class CreateUsuarioCommandHandler : ICommandHandler<CreateUsuarioCommand, UsuarioDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IContextoEmpresa _contextoEmpresa;

    public CreateUsuarioCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, IPasswordHasher passwordHasher, IContextoEmpresa contextoEmpresa)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _passwordHasher = passwordHasher;
        _contextoEmpresa = contextoEmpresa;
    }

    public async Task<UsuarioDto> Handle(CreateUsuarioCommand request, CancellationToken cancellationToken)
    {
        PasswordPolicy.Validar(request.Password);

        var empresaId = await ResolverEmpresaAsync(request.EmpresaId, cancellationToken);

        // El dominio valida y normaliza los datos
        var usuario = Usuario.Crear(request.Email, _passwordHasher.Hash(request.Password), request.Estado, empresaId);

        if (await _unitOfWork.Usuarios.EmailExistsAsync(usuario.Email, null, cancellationToken))
            throw new ConflictException($"Ya existe un usuario con el email '{usuario.Email}'.");

        await _unitOfWork.Usuarios.AddAsync(usuario, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // UsuarioId y FechaRegistro ya vienen asignados por la base de datos
        return _mapper.Map<UsuarioDto>(usuario);
    }

    private async Task<Guid?> ResolverEmpresaAsync(Guid? solicitada, CancellationToken cancellationToken)
    {
        if (!_contextoEmpresa.VeTodasLasEmpresas)
        {
            if (solicitada is not null && solicitada != _contextoEmpresa.EmpresaId)
                throw new ForbiddenException("Solo puedes crear usuarios de tu empresa.");

            return _contextoEmpresa.EmpresaId;
        }

        if (solicitada is not null && !await _unitOfWork.Empresas.ExistsAsync(solicitada.Value, cancellationToken))
            throw new NotFoundException($"No existe una empresa con el id {solicitada}.");

        return solicitada;
    }
}
