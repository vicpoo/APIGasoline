using APIGasoline.Application.Features.Viajes.DTOs;
using APIGasoline.Domain.Entities;
using APIGasoline.Domain.Interfaces;
using AutoMapper;

namespace APIGasoline.Application.Features.Viajes.Common;

/// <summary>
/// Agrega a los <see cref="ViajeDto"/> sus pausas, si está en pausa y el tiempo total en pausa.
/// Una sola consulta para toda la lista (no una por viaje).
/// </summary>
public static class PausasViaje
{
    public static async Task<ViajeDto> ConPausasAsync(
        this ViajeDto dto,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        DateTime ahora,
        CancellationToken cancellationToken)
        => (await new[] { dto }.ConPausasAsync(unitOfWork, mapper, ahora, cancellationToken))[0];

    public static async Task<IReadOnlyList<ViajeDto>> ConPausasAsync(
        this IReadOnlyList<ViajeDto> dtos,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        DateTime ahora,
        CancellationToken cancellationToken)
    {
        if (dtos.Count == 0)
            return dtos;

        var pausas = await unitOfWork.ViajePausas.GetByViajesAsync(dtos.Select(v => v.ViajeId).ToList(), cancellationToken);
        var porViaje = pausas.ToLookup(p => p.ViajeId);

        return dtos
            .Select(dto => Aplicar(dto, porViaje[dto.ViajeId].ToList(), mapper, ahora))
            .ToList();
    }

    private static ViajeDto Aplicar(ViajeDto dto, IReadOnlyList<ViajePausa> pausas, IMapper mapper, DateTime ahora)
        => dto with
        {
            EstaPausado = pausas.Any(p => p.EstaAbierta),
            SegundosEnPausa = (long)pausas.Sum(p => p.Duracion(ahora).TotalSeconds),
            Pausas = mapper.Map<IReadOnlyList<ViajePausaDto>>(pausas)
        };
}
