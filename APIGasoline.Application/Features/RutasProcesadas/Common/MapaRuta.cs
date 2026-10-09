using APIGasoline.Application.Features.RutasProcesadas.DTOs;
using APIGasoline.Domain.Rutas;

namespace APIGasoline.Application.Features.RutasProcesadas.Common;

public static class MapaRuta
{
    public static IReadOnlyList<CoordenadaDto> Coordenadas(string? polyline)
        => string.IsNullOrEmpty(polyline)
            ? []
            : CodificadorPolyline.Decodificar(polyline)
                .Select(p => new CoordenadaDto(p.Latitud, p.Longitud))
                .ToList();
}
