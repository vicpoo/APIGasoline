using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Domain.Interfaces;
using MediatR;

namespace APIGasoline.Application.Features.Empresas.Commands;

/// <summary>
/// Soft delete: marca la empresa como eliminada (delete = true), no borra el registro.
/// </summary>
public record DeleteEmpresaCommand(Guid EmpresaId) : IRequest;

public class DeleteEmpresaCommandHandler : IRequestHandler<DeleteEmpresaCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteEmpresaCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(DeleteEmpresaCommand request, CancellationToken cancellationToken)
    {
        var empresa = await _unitOfWork.Empresas.GetByIdAsync(request.EmpresaId, cancellationToken)
            ?? throw new NotFoundException($"No existe una empresa con el id {request.EmpresaId}.");

        empresa.Eliminar();
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
