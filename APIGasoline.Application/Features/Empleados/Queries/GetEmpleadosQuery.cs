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

    public GetEmpleadosQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<EmpleadoDto>> Handle(GetEmpleadosQuery request, CancellationToken cancellationToken)
    {
        var empleados = await _unitOfWork.Empleados.GetAllAsync(request.EmpresaId, cancellationToken);
        return _mapper.Map<IReadOnlyList<EmpleadoDto>>(empleados);
    }
}
