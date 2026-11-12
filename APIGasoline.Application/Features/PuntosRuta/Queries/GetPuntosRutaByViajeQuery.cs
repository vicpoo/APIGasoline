using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Application.Common.Interfaces;
using APIGasoline.Application.Features.Viajes.Common;
using APIGasoline.Application.Features.PuntosRuta.DTOs;
using APIGasoline.Domain.Interfaces;
using AutoMapper;
using APIGasoline.Application.Common.Messaging;

namespace APIGasoline.Application.Features.PuntosRuta.Queries;

/// <summary>
/// Puntos del viaje en orden de captura, listos para dibujar la ruta en el mapa.
/// </summary>
public sealed record GetPuntosRutaByViajeQuery(Guid ViajeId) : IQuery<IReadOnlyList<PuntoRutaDto>>;

public sealed class GetPuntosRutaByViajeQueryHandler : IQueryHandler<GetPuntosRutaByViajeQuery, IReadOnlyList<PuntoRutaDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IUsuarioActual _usuarioActual;

    public GetPuntosRutaByViajeQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, IUsuarioActual usuarioActual)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _usuarioActual = usuarioActual;
    }

    public async Task<IReadOnlyList<PuntoRutaDto>> Handle(GetPuntosRutaByViajeQuery request, CancellationToken cancellationToken)
    {
        var viaje = await _unitOfWork.Viajes.GetByIdAsync(request.ViajeId, cancellationToken)
            ?? throw new NotFoundException($"No existe un viaje con el id {request.ViajeId}.");

        // El rastro GPS es del operador del viaje (o de quien supervisa o audita)
        await AccesoViaje.ValidarLecturaDetalleAsync(_usuarioActual, viaje, cancellationToken);

        var puntos = await _unitOfWork.PuntosRuta.GetByViajeAsync(request.ViajeId, cancellationToken);
        return _mapper.Map<IReadOnlyList<PuntoRutaDto>>(puntos);
    }
}
