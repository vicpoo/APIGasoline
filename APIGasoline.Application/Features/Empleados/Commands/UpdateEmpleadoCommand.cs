using APIGasoline.Application.Common.Interfaces;
using APIGasoline.Application.Common.Storage;
using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Application.Features.Empleados.Common;
using APIGasoline.Application.Features.Empleados.DTOs;
using APIGasoline.Domain.Interfaces;
using AutoMapper;
using MediatR;

namespace APIGasoline.Application.Features.Empleados.Commands;

/// <summary>
/// Actualiza los datos del empleado. Las fotos NO se cambian aquí: usa SubirFotoEmpleadoCommand / EliminarFotoEmpleadoCommand.
/// </summary>
public record UpdateEmpleadoCommand(
    Guid EmpleadoId,
    Guid EmpresaId,
    Guid UsuarioId,
    string NombreCompleto) : IRequest<EmpleadoDto>;

public class UpdateEmpleadoCommandHandler : IRequestHandler<UpdateEmpleadoCommand, EmpleadoDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IFileStorageService _storage;

    public UpdateEmpleadoCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, IFileStorageService storage)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _storage = storage;
    }

    public async Task<EmpleadoDto> Handle(UpdateEmpleadoCommand request, CancellationToken cancellationToken)
    {
        var empleado = await _unitOfWork.Empleados.GetByIdAsync(request.EmpleadoId, cancellationToken)
            ?? throw new NotFoundException($"No existe un empleado con el id {request.EmpleadoId}.");

        // El dominio valida y normaliza los datos
        empleado.CambiarEmpresa(request.EmpresaId);
        empleado.CambiarUsuario(request.UsuarioId);
        empleado.CambiarNombreCompleto(request.NombreCompleto);

        await EmpleadoValidator.ValidarAsync(_unitOfWork, empleado.EmpresaId, empleado.UsuarioId, empleado.EmpleadoId, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await _mapper.Map<EmpleadoDto>(empleado).ConUrlsAsync(_storage, cancellationToken);
    }
}
