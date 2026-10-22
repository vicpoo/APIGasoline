using APIGasoline.Application.Common.Interfaces;
using APIGasoline.Application.Features.SolicitudesReserva.DTOs;
using APIGasoline.Domain.Entities;
using APIGasoline.Domain.Interfaces;

namespace APIGasoline.Application.Features.SolicitudesReserva.Common;

/// <summary>
/// Completa las solicitudes con los datos que la app muestra (nombre y foto del solicitante,
/// carro y licencia). Hace una sola consulta de empleados y una de carros para toda la lista
/// (sin N+1) y genera las URLs temporales de las fotos con IFileStorageService.
/// </summary>
internal static class DatosSolicitud
{
    /// <summary>Lista: agrega Solicitante (nombre y foto) y Carro (resumen) a cada solicitud.</summary>
    public static async Task<IReadOnlyList<SolicitudReservaDto>> ConDatosAsync(
        this IReadOnlyList<SolicitudReservaDto> solicitudes,
        IUnitOfWork unitOfWork,
        IFileStorageService storage,
        CancellationToken cancellationToken)
    {
        if (solicitudes.Count == 0)
            return solicitudes;

        var empleados = (await unitOfWork.Empleados.GetByIdsAsync(
                solicitudes.Select(s => s.EmpleadoSolicitanteId), cancellationToken))
            .ToDictionary(e => e.EmpleadoId);

        var carros = (await unitOfWork.Carros.GetByIdsAsync(
                solicitudes.Select(s => s.CarroId), cancellationToken))
            .ToDictionary(c => c.CarroId);

        var urls = new UrlsCache(storage);
        var resultado = new List<SolicitudReservaDto>(solicitudes.Count);

        foreach (var solicitud in solicitudes)
        {
            empleados.TryGetValue(solicitud.EmpleadoSolicitanteId, out var empleado);
            carros.TryGetValue(solicitud.CarroId, out var carro);

            resultado.Add(solicitud with
            {
                Solicitante = empleado is null ? null : await ResumenAsync(empleado, urls, cancellationToken),
                Carro = carro is null ? null : new CarroResumenDto(
                    carro.CarroId, carro.Marca, carro.Modelo, carro.Anio, carro.Color, carro.Placas,
                    await urls.ObtenerAsync(carro.FotoVehiculo, cancellationToken))
            });
        }

        return resultado;
    }

    public static async Task<SolicitudReservaDto> ConDatosAsync(
        this SolicitudReservaDto solicitud,
        IUnitOfWork unitOfWork,
        IFileStorageService storage,
        CancellationToken cancellationToken)
        => (await new[] { solicitud }.ConDatosAsync(unitOfWork, storage, cancellationToken))[0];

    /// <summary>Detalle: solicitante con licencia, carro completo y confirmador.</summary>
    public static async Task<SolicitudReservaDetalleDto> DetalleAsync(
        SolicitudReserva solicitud,
        IUnitOfWork unitOfWork,
        IFileStorageService storage,
        CancellationToken cancellationToken)
    {
        var ids = new List<Guid> { solicitud.EmpleadoSolicitanteId };
        if (solicitud.EmpleadoConfirmadorId is { } confirmadorId)
            ids.Add(confirmadorId);

        var empleados = (await unitOfWork.Empleados.GetByIdsAsync(ids, cancellationToken))
            .ToDictionary(e => e.EmpleadoId);

        var carro = (await unitOfWork.Carros.GetByIdsAsync([solicitud.CarroId], cancellationToken)).FirstOrDefault();
        var urls = new UrlsCache(storage);

        SolicitanteDetalleDto? solicitante = null;
        if (empleados.TryGetValue(solicitud.EmpleadoSolicitanteId, out var s))
        {
            solicitante = new SolicitanteDetalleDto(
                s.EmpleadoId,
                s.NombreCompleto,
                await urls.ObtenerAsync(s.FotoPerfil, cancellationToken),
                s.TieneLicencia,
                await urls.ObtenerAsync(s.FotoLicencia, cancellationToken),
                s.FechaCargaLicencia);
        }

        EmpleadoResumenDto? confirmador = null;
        if (solicitud.EmpleadoConfirmadorId is { } idConfirmador && empleados.TryGetValue(idConfirmador, out var c))
            confirmador = await ResumenAsync(c, urls, cancellationToken);

        CarroDetalleDto? carroDetalle = carro is null ? null : new CarroDetalleDto(
            carro.CarroId, carro.Marca, carro.Modelo, carro.Anio, carro.Color, carro.Placas,
            carro.KilometrajeActual, carro.Estado, carro.DadoDeBaja,
            await urls.ObtenerAsync(carro.FotoVehiculo, cancellationToken));

        return new SolicitudReservaDetalleDto(
            solicitud.SolicitudReservaId,
            solicitud.EmpresaId,
            solicitud.CarroId,
            solicitud.EmpleadoSolicitanteId,
            solicitud.MotivoSolicitud,
            solicitud.FechaSolicitud,
            solicitud.Estado,
            solicitante,
            carroDetalle,
            solicitud.EmpleadoConfirmadorId,
            confirmador,
            solicitud.FechaResolucion,
            solicitud.MotivoResolucion,
            solicitud.FechaLiberacion,
            solicitud.ObservacionesLiberacion);
    }

    private static async Task<EmpleadoResumenDto> ResumenAsync(Empleado empleado, UrlsCache urls, CancellationToken cancellationToken)
        => new(empleado.EmpleadoId, empleado.NombreCompleto, await urls.ObtenerAsync(empleado.FotoPerfil, cancellationToken));

    /// <summary>Una URL por llave: si el mismo empleado o carro aparece en varias solicitudes, se firma una vez.</summary>
    private sealed class UrlsCache(IFileStorageService storage)
    {
        private readonly Dictionary<string, string?> _urls = new();

        public async Task<string?> ObtenerAsync(string? llave, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(llave))
                return null;

            if (!_urls.TryGetValue(llave, out var url))
            {
                url = await storage.ObtenerUrlAsync(llave, cancellationToken);
                _urls[llave] = url;
            }

            return url;
        }
    }
}
