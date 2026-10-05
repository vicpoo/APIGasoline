using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Application.Features.Empleados.DTOs;
using APIGasoline.Domain.Interfaces;
using AutoMapper;
using MediatR;

namespace APIGasoline.Application.Features.Empleados.Queries;

/// <summary>
/// Obtiene el empleado ligado a un usuario del servicio de seguridad
/// (útil al iniciar sesión en la app).
/// </summary>
public record GetEmpleadoByUsuarioIdQuery(int UsuarioId) : IRequest<EmpleadoDto>;

public class GetEmpleadoByUsuarioIdQueryHandler : IRequestHandler<GetEmpleadoByUsuarioIdQuery, EmpleadoDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetEmpleadoByUsuarioIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<EmpleadoDto> Handle(GetEmpleadoByUsuarioIdQuery request, CancellationToken cancellationToken)
    {
        var empleado = await _unitOfWork.Empleados.GetByUsuarioIdAsync(request.UsuarioId, cancellationToken)
            ?? throw new NotFoundException($"No existe un empleado para el usuario {request.UsuarioId}.");

        return _mapper.Map<EmpleadoDto>(empleado);
    }
}
