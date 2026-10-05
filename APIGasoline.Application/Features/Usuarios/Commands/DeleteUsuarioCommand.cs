using System;
using System.Collections.Generic;
using System.Text;
using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Domain.Interfaces;
using MediatR;

namespace APIGasoline.Application.Features.Usuarios.Commands;

public record DeleteUsuarioCommand(int UsuarioId) : IRequest;

public class DeleteUsuarioCommandHandler : IRequestHandler<DeleteUsuarioCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteUsuarioCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(DeleteUsuarioCommand request, CancellationToken cancellationToken)
    {
        var usuario = await _unitOfWork.Usuarios.GetByIdAsync(request.UsuarioId, cancellationToken)
            ?? throw new NotFoundException($"No existe un usuario con el id {request.UsuarioId}.");

        if (await _unitOfWork.Empleados.UsuarioAsignadoAsync(usuario.UsuarioId, null, cancellationToken))
            throw new ConflictException("No se puede eliminar el usuario porque está asignado a un empleado.");

        _unitOfWork.Usuarios.Remove(usuario);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}


