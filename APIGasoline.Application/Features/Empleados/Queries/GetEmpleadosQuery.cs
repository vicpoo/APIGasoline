using APIGasoline.Application.Common.Security;
using APIGasoline.Application.Common.Interfaces;
using APIGasoline.Application.Common.Storage;
using APIGasoline.Application.Features.Empleados.DTOs;
using APIGasoline.Domain.Interfaces;
using AutoMapper;
using MediatR;

namespace APIGasoline.Application.Features.Empleados.Queries;

/// <summary>
/// Lista los empleados no eliminados. Se puede filtrar por empresa.
/// </summary>
public record GetEmpleadosQuery(Guid? EmpresaId = null) : IRequest<IReadOnlyList<EmpleadoDto>>;

public class GetEmpleadosQueryHandler : IRequestHandler<GetEmpleadosQuery, IReadOnlyList<EmpleadoDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IFileStorageService _storage;
    private readonly IUsuarioActual _usuarioActual;

    public GetEmpleadosQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, IFileStorageService storage, IUsuarioActual usuarioActual)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _storage = storage;
        _usuarioActual = usuarioActual;
    }

    public async Task<IReadOnlyList<EmpleadoDto>> Handle(GetEmpleadosQuery request, CancellationToken cancellationToken)
    {
        var empresaId = await _usuarioActual.ResolverEmpresaAsync(request.EmpresaId, cancellationToken);

        var empleados = await _unitOfWork.Empleados.GetAllAsync(empresaId, cancellationToken);
        return await _mapper.Map<IReadOnlyList<EmpleadoDto>>(empleados).ConUrlsAsync(_storage, cancellationToken);
    }
}
