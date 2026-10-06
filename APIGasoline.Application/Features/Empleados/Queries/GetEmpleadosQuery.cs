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
public record GetEmpleadosQuery(int? EmpresaId = null) : IRequest<IReadOnlyList<EmpleadoDto>>;

public class GetEmpleadosQueryHandler : IRequestHandler<GetEmpleadosQuery, IReadOnlyList<EmpleadoDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IFileStorageService _storage;

    public GetEmpleadosQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, IFileStorageService storage)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _storage = storage;
    }

    public async Task<IReadOnlyList<EmpleadoDto>> Handle(GetEmpleadosQuery request, CancellationToken cancellationToken)
    {
        var empleados = await _unitOfWork.Empleados.GetAllAsync(request.EmpresaId, cancellationToken);
        return await _mapper.Map<IReadOnlyList<EmpleadoDto>>(empleados).ConUrlsAsync(_storage, cancellationToken);
    }
}
