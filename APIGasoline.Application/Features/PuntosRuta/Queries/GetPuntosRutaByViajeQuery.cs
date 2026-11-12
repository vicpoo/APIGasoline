using APIGasoline.Domain.Common;
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
/// <summary>
/// Puntos GPS de un viaje en orden de captura, por páginas (F011). Un punto pesa poco y la app arma la
/// ruta con ellos, así que la página es grande: 1000 por defecto, máximo 5000.
/// </summary>
public sealed record GetPuntosRutaByViajeQuery(Guid ViajeId, int? Pagina = null, int? TamanoPagina = null) : IQuery<PaginaDe<PuntoRutaDto>>
{
    public const int TamanoPorDefecto = 1000;
    public const int TamanoMaximo = 5000;
}

public sealed class GetPuntosRutaByViajeQueryHandler : IQueryHandler<GetPuntosRutaByViajeQuery, PaginaDe<PuntoRutaDto>>
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

    public async Task<PaginaDe<PuntoRutaDto>> Handle(GetPuntosRutaByViajeQuery request, CancellationToken cancellationToken)
    {
        var viaje = await _unitOfWork.Viajes.GetByIdAsync(request.ViajeId, cancellationToken)
            ?? throw new NotFoundException($"No existe un viaje con el id {request.ViajeId}.");

        // El rastro GPS es del operador del viaje (o de quien supervisa o audita)
        await AccesoViaje.ValidarLecturaDetalleAsync(_usuarioActual, viaje, cancellationToken);

        var paginacion = new Paginacion(request.Pagina, request.TamanoPagina,
            GetPuntosRutaByViajeQuery.TamanoPorDefecto, GetPuntosRutaByViajeQuery.TamanoMaximo);

        var pagina = await _unitOfWork.PuntosRuta.GetPaginaByViajeAsync(request.ViajeId, paginacion, cancellationToken);
        return pagina.Con(_mapper.Map<IReadOnlyList<PuntoRutaDto>>(pagina.Elementos));
    }
}

/// <summary>Todos los puntos del viaje (para exportar la ruta a GPX). Mismo acceso que el listado.</summary>
public sealed record GetRutaCompletaViajeQuery(Guid ViajeId) : IQuery<IReadOnlyList<PuntoRutaDto>>;

public sealed class GetRutaCompletaViajeQueryHandler : IQueryHandler<GetRutaCompletaViajeQuery, IReadOnlyList<PuntoRutaDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IUsuarioActual _usuarioActual;

    public GetRutaCompletaViajeQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, IUsuarioActual usuarioActual)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _usuarioActual = usuarioActual;
    }

    public async Task<IReadOnlyList<PuntoRutaDto>> Handle(GetRutaCompletaViajeQuery request, CancellationToken cancellationToken)
    {
        var viaje = await _unitOfWork.Viajes.GetByIdAsync(request.ViajeId, cancellationToken)
            ?? throw new NotFoundException($"No existe un viaje con el id {request.ViajeId}.");

        await AccesoViaje.ValidarLecturaDetalleAsync(_usuarioActual, viaje, cancellationToken);

        var puntos = await _unitOfWork.PuntosRuta.GetByViajeAsync(request.ViajeId, cancellationToken);
        return _mapper.Map<IReadOnlyList<PuntoRutaDto>>(puntos);
    }
}
