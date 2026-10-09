using APIGasoline.Application.Features.SolicitudesReserva.DTOs;
using APIGasoline.Domain.Enums;
using APIGasoline.Domain.Interfaces;
using AutoMapper;
using MediatR;

namespace APIGasoline.Application.Features.SolicitudesReserva.Queries;

/// <summary>
/// Lista las solicitudes de la más antigua a la más reciente (cola de confirmación).
/// Filtros opcionales: empresa, carro, empleado solicitante y estado.
/// </summary>
public record GetSolicitudesReservaQuery(
    Guid? EmpresaId = null,
    Guid? CarroId = null,
    Guid? EmpleadoSolicitanteId = null,
    EstadoSolicitudReserva? Estado = null) : IRequest<IReadOnlyList<SolicitudReservaDto>>;

public class GetSolicitudesReservaQueryHandler : IRequestHandler<GetSolicitudesReservaQuery, IReadOnlyList<SolicitudReservaDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetSolicitudesReservaQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<SolicitudReservaDto>> Handle(GetSolicitudesReservaQuery request, CancellationToken cancellationToken)
    {
        var solicitudes = await _unitOfWork.SolicitudesReserva.GetAllAsync(
            request.EmpresaId, request.CarroId, request.EmpleadoSolicitanteId, request.Estado, cancellationToken);

        return _mapper.Map<IReadOnlyList<SolicitudReservaDto>>(solicitudes);
    }
}
