using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Application.Features.RutasProcesadas.DTOs;
using APIGasoline.Domain.Interfaces;
using AutoMapper;
using MediatR;

namespace APIGasoline.Application.Features.RutasProcesadas.Queries;

/// <summary>
/// Resumen procesado de un viaje. Si el viaje existe pero aún no se procesa
/// (sigue en curso o falló el proceso), responde 404 con un mensaje claro.
/// </summary>
public record GetRutaProcesadaByViajeQuery(int ViajeId) : IRequest<RutaProcesadaDto>;

public class GetRutaProcesadaByViajeQueryHandler : IRequestHandler<GetRutaProcesadaByViajeQuery, RutaProcesadaDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetRutaProcesadaByViajeQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<RutaProcesadaDto> Handle(GetRutaProcesadaByViajeQuery request, CancellationToken cancellationToken)
    {
        var viaje = await _unitOfWork.Viajes.GetByIdAsync(request.ViajeId, cancellationToken)
            ?? throw new NotFoundException($"No existe un viaje con el id {request.ViajeId}.");

        var ruta = await _unitOfWork.RutasProcesadas.GetByViajeAsync(viaje.ViajeId, cancellationToken)
            ?? throw new NotFoundException(viaje.EstaEnCurso
                ? $"El viaje {viaje.ViajeId} sigue en curso; su ruta se procesa al cerrarlo."
                : $"El viaje {viaje.ViajeId} aún no tiene ruta procesada. Use el endpoint de recalcular.");

        return _mapper.Map<RutaProcesadaDto>(ruta);
    }
}
