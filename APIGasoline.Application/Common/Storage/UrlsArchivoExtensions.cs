using APIGasoline.Application.Common.Interfaces;
using APIGasoline.Application.Features.CargasCombustible.DTOs;
using APIGasoline.Application.Features.Carros.DTOs;
using APIGasoline.Application.Features.Empleados.DTOs;

namespace APIGasoline.Application.Common.Storage;

/// <summary>
/// Completa las URLs temporales de los DTOs después de mapearlos con AutoMapper.
/// (AutoMapper solo copia datos; no debe llamar servicios externos.)
/// </summary>
public static class UrlsArchivoExtensions
{
    // ---------- Carros ----------
    public static async Task<CarroDto> ConUrlsAsync(this CarroDto dto, IFileStorageService storage, CancellationToken cancellationToken)
        => dto with { FotoVehiculoUrl = await storage.ObtenerUrlAsync(dto.FotoVehiculo, cancellationToken) };

    public static async Task<IReadOnlyList<CarroDto>> ConUrlsAsync(this IEnumerable<CarroDto> dtos, IFileStorageService storage, CancellationToken cancellationToken)
    {
        var resultado = new List<CarroDto>();
        foreach (var dto in dtos)
            resultado.Add(await dto.ConUrlsAsync(storage, cancellationToken));
        return resultado;
    }

    // ---------- Empleados ----------
    public static async Task<EmpleadoDto> ConUrlsAsync(this EmpleadoDto dto, IFileStorageService storage, CancellationToken cancellationToken)
        => dto with
        {
            FotoPerfilUrl = await storage.ObtenerUrlAsync(dto.FotoPerfil, cancellationToken),
            FotoLicenciaUrl = await storage.ObtenerUrlAsync(dto.FotoLicencia, cancellationToken)
        };

    public static async Task<IReadOnlyList<EmpleadoDto>> ConUrlsAsync(this IEnumerable<EmpleadoDto> dtos, IFileStorageService storage, CancellationToken cancellationToken)
    {
        var resultado = new List<EmpleadoDto>();
        foreach (var dto in dtos)
            resultado.Add(await dto.ConUrlsAsync(storage, cancellationToken));
        return resultado;
    }

    // ---------- Cargas de combustible ----------
    public static async Task<CargaCombustibleDto> ConUrlsAsync(this CargaCombustibleDto dto, IFileStorageService storage, CancellationToken cancellationToken)
        => dto with { FotoComprobanteUrl = await storage.ObtenerUrlAsync(dto.FotoComprobante, cancellationToken) };

    public static async Task<IReadOnlyList<CargaCombustibleDto>> ConUrlsAsync(this IEnumerable<CargaCombustibleDto> dtos, IFileStorageService storage, CancellationToken cancellationToken)
    {
        var resultado = new List<CargaCombustibleDto>();
        foreach (var dto in dtos)
            resultado.Add(await dto.ConUrlsAsync(storage, cancellationToken));
        return resultado;
    }
}
