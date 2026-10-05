using APIGasoline.Application.Features.Empleados.Common;
using APIGasoline.Application.Features.Empleados.DTOs;
using APIGasoline.Domain.Entities;
using APIGasoline.Domain.Interfaces;
using AutoMapper;
using MediatR;

namespace APIGasoline.Application.Features.Empleados.Commands;

public record CreateEmpleadoCommand(
    int EmpresaId,
    int UsuarioId,
    string NombreCompleto,
    string? FotoPerfil,
    string? FotoLicencia) : IRequest<EmpleadoDto>;

public class CreateEmpleadoCommandHandler : IRequestHandler<CreateEmpleadoCommand, EmpleadoDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateEmpleadoCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<EmpleadoDto> Handle(CreateEmpleadoCommand request, CancellationToken cancellationToken)
    {
        // El dominio valida y normaliza los datos
        var empleado = Empleado.Crear(request.EmpresaId, request.UsuarioId, request.NombreCompleto, request.FotoPerfil, request.FotoLicencia);

        await EmpleadoValidator.ValidarAsync(_unitOfWork, empleado.EmpresaId, empleado.UsuarioId, null, cancellationToken);

        await _unitOfWork.Empleados.AddAsync(empleado, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // EmpleadoId y FechaAlta ya vienen asignados por la base de datos
        return _mapper.Map<EmpleadoDto>(empleado);
    }
}
