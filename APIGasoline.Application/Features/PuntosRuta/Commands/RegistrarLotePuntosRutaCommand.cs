using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Application.Common.Interfaces;
using APIGasoline.Application.Features.PuntosRuta.DTOs;
using APIGasoline.Domain.Entities;
using APIGasoline.Domain.Interfaces;
using MediatR;

namespace APIGasoline.Application.Features.PuntosRuta.Commands;

/// <summary>
/// Registra varios puntos GPS de un viaje en una sola llamada.
/// - La empresa se toma del viaje (no se confía en la app).
/// - Es idempotente: los puntos con un timestamp ya registrado para el viaje se ignoran,
///   así la app puede reenviar un lote si se cortó la conexión.
/// - Todo el lote se guarda en un solo SaveChanges (si un punto es inválido, no se guarda ninguno).
/// </summary>
public record RegistrarLotePuntosRutaCommand(
    int ViajeId,
    bool Sincronizado,
    IReadOnlyList<PuntoRutaNuevoDto> Puntos) : IRequest<LotePuntosRutaResultadoDto>;

public class RegistrarLotePuntosRutaCommandHandler : IRequestHandler<RegistrarLotePuntosRutaCommand, LotePuntosRutaResultadoDto>
{
    public const int MaxPuntosPorLote = 1000;

    // Margen por diferencias de reloj entre el teléfono y el servidor
    private static readonly TimeSpan Tolerancia = TimeSpan.FromMinutes(5);

    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _dateTime;

    public RegistrarLotePuntosRutaCommandHandler(IUnitOfWork unitOfWork, IDateTimeProvider dateTime)
    {
        _unitOfWork = unitOfWork;
        _dateTime = dateTime;
    }

    public async Task<LotePuntosRutaResultadoDto> Handle(RegistrarLotePuntosRutaCommand request, CancellationToken cancellationToken)
    {
        if (request.Puntos.Count == 0)
            throw new BadRequestException("El lote no contiene puntos.");

        if (request.Puntos.Count > MaxPuntosPorLote)
            throw new BadRequestException($"Un lote no puede tener más de {MaxPuntosPorLote} puntos.");

        var viaje = await _unitOfWork.Viajes.GetByIdAsync(request.ViajeId, cancellationToken)
            ?? throw new NotFoundException($"No existe un viaje con el id {request.ViajeId}.");

        // Los puntos deben caer dentro del periodo del viaje (con tolerancia)
        var desde = viaje.FechaInicio - Tolerancia;
        var hasta = (viaje.FechaFin ?? _dateTime.Now) + Tolerancia;

        var existentes = await _unitOfWork.PuntosRuta.GetTimestampsByViajeAsync(viaje.ViajeId, cancellationToken);
        var nuevos = new List<PuntoRuta>();

        foreach (var punto in request.Puntos)
        {
            // El dominio valida coordenadas y normaliza el timestamp
            var puntoRuta = PuntoRuta.Crear(
                viaje.EmpresaId,
                viaje.ViajeId,
                punto.Latitud,
                punto.Longitud,
                punto.TimestampCaptura,
                request.Sincronizado);

            if (puntoRuta.TimestampCaptura < desde || puntoRuta.TimestampCaptura > hasta)
                throw new BadRequestException(
                    $"El punto capturado el {puntoRuta.TimestampCaptura:yyyy-MM-dd HH:mm:ss} está fuera del periodo del viaje.");

            // Add devuelve false si ya estaba: cubre duplicados en la BD y dentro del mismo lote
            if (existentes.Add(puntoRuta.TimestampCaptura))
                nuevos.Add(puntoRuta);
        }

        if (nuevos.Count > 0)
        {
            await _unitOfWork.PuntosRuta.AddRangeAsync(nuevos, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return new LotePuntosRutaResultadoDto(
            viaje.ViajeId,
            request.Puntos.Count,
            nuevos.Count,
            request.Puntos.Count - nuevos.Count);
    }
}
