using APIGasoline.Application.Common.Security;
using APIGasoline.Application.Common.Interfaces;
using APIGasoline.Application.Common.Storage;
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
public record GetEmpleadoByUsuarioIdQuery(Guid UsuarioId) : IRequest<EmpleadoDto>, IRecursoPropio
{
    /// <summary>Su propio perfil, o cualquiera con empleado.ver.</summary>
    public bool EsPropio(IUsuarioActual usuario)
        => UsuarioId == usuario.UsuarioId;

    public string PrivilegioParaOtros => Permisos.EmpleadoVer;
}

public class GetEmpleadoByUsuarioIdQueryHandler : IRequestHandler<GetEmpleadoByUsuarioIdQuery, EmpleadoDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IFileStorageService _storage;

    public GetEmpleadoByUsuarioIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, IFileStorageService storage)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _storage = storage;
    }

    public async Task<EmpleadoDto> Handle(GetEmpleadoByUsuarioIdQuery request, CancellationToken cancellationToken)
    {
        var empleado = await _unitOfWork.Empleados.GetByUsuarioIdAsync(request.UsuarioId, cancellationToken)
            ?? throw new NotFoundException($"No existe un empleado para el usuario {request.UsuarioId}.");

        return await _mapper.Map<EmpleadoDto>(empleado).ConUrlsAsync(_storage, cancellationToken);
    }
}
