using APIGasoline.Application.Features.Viajes.DTOs;
using APIGasoline.Domain.Enums;
using APIGasoline.Domain.Interfaces;
using AutoMapper;
using MediatR;

namespace APIGasoline.Application.Features.Viajes.Queries;

/// <summary>
/// Lista los viajes del más reciente al más antiguo.
/// Filtros opcionales: empresa, carro, operador, solicitud de reserva y estado.
/// </summary>
public record GetViajesQuery(
    int? EmpresaId = null,
    int? CarroId = null,
    int? EmpleadoOperadorId = null,
    int? SolicitudReservaId = null,
    EstadoViaje? Estado = null) : IRequest<IReadOnlyList<ViajeDto>>;

public class GetViajesQueryHandler : IRequestHandler<GetViajesQuery, IReadOnlyList<ViajeDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetViajesQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<ViajeDto>> Handle(GetViajesQuery request, CancellationToken cancellationToken)
    {
        var viajes = await _unitOfWork.Viajes.GetAllAsync(
            request.EmpresaId, request.CarroId, request.EmpleadoOperadorId, request.SolicitudReservaId, request.Estado, cancellationToken);

        return _mapper.Map<IReadOnlyList<ViajeDto>>(viajes);
    }
}
