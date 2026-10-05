using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Application.Features.Empleados.Common;
using APIGasoline.Application.Features.Empleados.DTOs;
using APIGasoline.Domain.Interfaces;
using AutoMapper;
using MediatR;

namespace APIGasoline.Application.Features.Empleados.Commands;

public record UpdateEmpleadoCommand(
    int EmpleadoId,
    int EmpresaId,
    int UsuarioId,
    string NombreCompleto,
    string? FotoPerfil,
    string? FotoLicencia) : IRequest<EmpleadoDto>;

public class UpdateEmpleadoCommandHandler : IRequestHandler<UpdateEmpleadoCommand, EmpleadoDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UpdateEmpleadoCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<EmpleadoDto> Handle(UpdateEmpleadoCommand request, CancellationToken cancellationToken)
    {
        var empleado = await _unitOfWork.Empleados.GetByIdAsync(request.EmpleadoId, cancellationToken)
            ?? throw new NotFoundException($"No existe un empleado con el id {request.EmpleadoId}.");

        // El dominio valida y normaliza los datos
        empleado.CambiarEmpresa(request.EmpresaId);
        empleado.CambiarUsuario(request.UsuarioId);
        empleado.CambiarNombreCompleto(request.NombreCompleto);
        empleado.CambiarFotoPerfil(request.FotoPerfil);
        empleado.CambiarFotoLicencia(request.FotoLicencia);

        await EmpleadoValidator.ValidarAsync(_unitOfWork, empleado.EmpresaId, empleado.UsuarioId, empleado.EmpleadoId, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<EmpleadoDto>(empleado);
    }
}
