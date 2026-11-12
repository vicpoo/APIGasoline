using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Application.Common.Interfaces;
using APIGasoline.Application.Features.Viajes.Common;
using APIGasoline.Application.Features.PuntosRuta.DTOs;
using APIGasoline.Domain.Interfaces;
using AutoMapper;
using APIGasoline.Application.Common.Messaging;

namespace APIGasoline.Application.Features.PuntosRuta.Queries;

public sealed record GetPuntoRutaByIdQuery(Guid PuntoRutaId) : IQuery<PuntoRutaDto>;

public sealed class GetPuntoRutaByIdQueryHandler : IQueryHandler<GetPuntoRutaByIdQuery, PuntoRutaDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IUsuarioActual _usuarioActual;

    public GetPuntoRutaByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, IUsuarioActual usuarioActual)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _usuarioActual = usuarioActual;
    }

    public async Task<PuntoRutaDto> Handle(GetPuntoRutaByIdQuery request, CancellationToken cancellationToken)
    {
        var puntoRuta = await _unitOfWork.PuntosRuta.GetByIdAsync(request.PuntoRutaId, cancellationToken)
            ?? throw new NotFoundException($"No existe un punto de ruta con el id {request.PuntoRutaId}.");

        var viaje = await _unitOfWork.Viajes.GetByIdAsync(puntoRuta.ViajeId, cancellationToken)
            ?? throw new NotFoundException($"No existe un punto de ruta con el id {request.PuntoRutaId}.");

        await AccesoViaje.ValidarLecturaDetalleAsync(_usuarioActual, viaje, cancellationToken);

        return _mapper.Map<PuntoRutaDto>(puntoRuta);
    }
}
