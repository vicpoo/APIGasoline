using APIGasoline.Application.Common.Interfaces;
using APIGasoline.Application.Common.Security;
using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Application.Features.Dispositivos.DTOs;
using APIGasoline.Domain.Interfaces;
using AutoMapper;
using APIGasoline.Application.Common.Messaging;

namespace APIGasoline.Application.Features.Dispositivos.Queries;

public sealed record GetDispositivosByEmpleadoQuery(Guid EmpleadoId) : IQuery<IReadOnlyList<DispositivoDto>>, IRecursoPropio
{
    /// <summary>Sus celulares, o los de cualquiera con empleado.ver.</summary>
    public bool EsPropio(IUsuarioActual usuario)
        => EmpleadoId == usuario.EmpleadoId;

    public string PrivilegioParaOtros => Permisos.EmpleadoVer;
}

public sealed class GetDispositivosByEmpleadoQueryHandler : IQueryHandler<GetDispositivosByEmpleadoQuery, IReadOnlyList<DispositivoDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetDispositivosByEmpleadoQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<DispositivoDto>> Handle(GetDispositivosByEmpleadoQuery request, CancellationToken cancellationToken)
    {
        if (await _unitOfWork.Empleados.GetByIdAsync(request.EmpleadoId, cancellationToken) is null)
            throw new NotFoundException($"No existe un empleado con el id {request.EmpleadoId}.");

        var dispositivos = await _unitOfWork.Dispositivos.GetByEmpleadoAsync(request.EmpleadoId, cancellationToken);
        return _mapper.Map<IReadOnlyList<DispositivoDto>>(dispositivos);
    }
}
