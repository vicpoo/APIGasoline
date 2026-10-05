using System;
using System.Collections.Generic;
using System.Text;
using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Application.Features.Usuarios.DTOs;
using APIGasoline.Domain.Entities;
using APIGasoline.Domain.Enums;
using APIGasoline.Domain.Interfaces;
using AutoMapper;
using MediatR;

namespace APIGasoline.Application.Features.Usuarios.Commands;

public record CreateUsuarioCommand(int UsuarioId, string Email, EstadoUsuario Estado) : IRequest<UsuarioDto>;

public class CreateUsuarioCommandHandler : IRequestHandler<CreateUsuarioCommand, UsuarioDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateUsuarioCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<UsuarioDto> Handle(CreateUsuarioCommand request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim();

        if (await _unitOfWork.Usuarios.ExistsAsync(request.UsuarioId, cancellationToken))
            throw new ConflictException($"Ya existe un usuario con el id {request.UsuarioId}.");

        if (await _unitOfWork.Usuarios.EmailExistsAsync(email, null, cancellationToken))
            throw new ConflictException($"Ya existe un usuario con el email '{email}'.");

        var usuario = new Usuario
        {
            UsuarioId = request.UsuarioId,
            Email = email,
            Estado = request.Estado
        };

        await _unitOfWork.Usuarios.AddAsync(usuario, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<UsuarioDto>(usuario);
    }
}

