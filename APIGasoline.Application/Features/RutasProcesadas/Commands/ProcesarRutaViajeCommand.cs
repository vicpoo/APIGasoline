using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Application.Common.Interfaces;
using APIGasoline.Application.Features.RutasProcesadas.Common;
using APIGasoline.Application.Features.RutasProcesadas.DTOs;
using APIGasoline.Domain.Interfaces;
using AutoMapper;
using MediatR;

namespace APIGasoline.Application.Features.RutasProcesadas.Commands;

/// <summary>
/// Calcula (o recalcula) la ruta procesada de un viaje cerrado:
/// limpia los puntos GPS, mide distancia y tiempos, compara contra el odómetro,
/// calcula km/L y genera la polyline. Los puntos crudos no se modifican.
/// </summary>
public record ProcesarRutaViajeCommand(int ViajeId) : IRequest<RutaProcesadaDto>;

public class ProcesarRutaViajeCommandHandler : IRequestHandler<ProcesarRutaViajeCommand, RutaProcesadaDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IDateTimeProvider _dateTime;

    public ProcesarRutaViajeCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, IDateTimeProvider dateTime)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _dateTime = dateTime;
    }

    public async Task<RutaProcesadaDto> Handle(ProcesarRutaViajeCommand request, CancellationToken cancellationToken)
    {
        var viaje = await _unitOfWork.Viajes.GetByIdAsync(request.ViajeId, cancellationToken)
            ?? throw new NotFoundException($"No existe un viaje con el id {request.ViajeId}.");

        CalculoRutaViaje.ValidarViajeCerrado(viaje);

        var puntos = await _unitOfWork.PuntosRuta.GetByViajeAsync(viaje.ViajeId, cancellationToken);
        var litros = await _unitOfWork.CargasCombustible.SumLitrosByViajeAsync(viaje.ViajeId, cancellationToken);

        // Toda la lógica pesada (filtro, Haversine, Douglas-Peucker, polyline) vive en Domain
        var resultado = CalculoRutaViaje.Calcular(viaje, puntos, litros);

        var existente = await _unitOfWork.RutasProcesadas.GetByViajeAsync(viaje.ViajeId, cancellationToken);
        var ruta = CalculoRutaViaje.CrearOActualizar(existente, viaje.EmpresaId, viaje.ViajeId, resultado, _dateTime.Now);

        if (existente is null)
            await _unitOfWork.RutasProcesadas.AddAsync(ruta, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<RutaProcesadaDto>(ruta);
    }
}
