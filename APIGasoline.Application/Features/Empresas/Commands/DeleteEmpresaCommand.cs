using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Application.Common.Interfaces;
using APIGasoline.Application.Common.Sesiones;
using APIGasoline.Domain.Enums;
using APIGasoline.Domain.Interfaces;
using MediatR;

namespace APIGasoline.Application.Features.Empresas.Commands;

/// <summary>
/// Soft delete: marca la empresa como eliminada (delete = true), no borra el registro.
/// Cierra las sesiones abiertas de sus empleados.
/// </summary>
public record DeleteEmpresaCommand(Guid EmpresaId) : IRequest;

public class DeleteEmpresaCommandHandler : IRequestHandler<DeleteEmpresaCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUsuarioActual _usuarioActual;
    private readonly IDateTimeProvider _dateTime;

    public DeleteEmpresaCommandHandler(IUnitOfWork unitOfWork, IUsuarioActual usuarioActual, IDateTimeProvider dateTime)
    {
        _unitOfWork = unitOfWork;
        _usuarioActual = usuarioActual;
        _dateTime = dateTime;
    }

    public async Task Handle(DeleteEmpresaCommand request, CancellationToken cancellationToken)
    {
        var empresa = await _unitOfWork.Empresas.GetByIdAsync(request.EmpresaId, cancellationToken)
            ?? throw new NotFoundException($"No existe una empresa con el id {request.EmpresaId}.");

        empresa.Eliminar();

        await _unitOfWork.RevocarDeEmpresaAsync(
            empresa.EmpresaId, MotivoRevocacionSesion.EmpresaEliminada, _dateTime.Now, _usuarioActual.UsuarioId,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
