using APIGasoline.Application.Features.CambiosEstatusCarro.Commands;
using APIGasoline.Application.Features.CambiosEstatusCarro.DTOs;
using APIGasoline.Application.Features.CambiosEstatusCarro.Queries;
using APIGasoline.Contracts.Archivos;
using APIGasoline.Contracts.CambiosEstatusCarro;
using APIGasoline.Extensions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using APIGasoline.Application.Common.Security;
using APIGasoline.Authorization;

namespace APIGasoline.Controllers;

/// <summary>
/// Bitácora de estatus de carros. Solo se consulta y se agrega; no se edita ni se borra
/// (la única excepción es agregar la foto de evidencia, que puede llegar después).
/// Los registros con tipoMotivo fallaMecanica o choqueOGolpe forman el registro de percances.
/// </summary>
[ApiController]
[Route("api/cambios-estatus-carro")]
public class CambiosEstatusCarroController : ControllerBase
{
    private readonly ISender _sender;

    public CambiosEstatusCarroController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Historial del más reciente al más antiguo. Filtros opcionales: ?empresaId=, ?carroId=, ?desde=, ?hasta=
    /// </summary>
    [RequierePrivilegio(Permisos.CarroVer, Permisos.CarroCambiarEstatus)]
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CambioEstatusCarroDto>>> GetAll(
        [FromQuery] Guid? empresaId,
        [FromQuery] Guid? carroId,
        [FromQuery] DateTime? desde,
        [FromQuery] DateTime? hasta,
        CancellationToken cancellationToken)
    {
        var cambios = await _sender.Send(new GetCambiosEstatusCarroQuery(empresaId, carroId, desde, hasta), cancellationToken);
        return Ok(cambios);
    }

    [RequierePrivilegio(Permisos.CarroVer, Permisos.CarroCambiarEstatus)]
    [HttpGet("{cambioEstatusCarroId:guid}")]
    public async Task<ActionResult<CambioEstatusCarroDto>> GetById(Guid cambioEstatusCarroId, CancellationToken cancellationToken)
    {
        var cambio = await _sender.Send(new GetCambioEstatusCarroByIdQuery(cambioEstatusCarroId), cancellationToken);
        return Ok(cambio);
    }

    /// <summary>
    /// Cambia el estatus del carro (disponible / enMantenimiento) y lo registra en la bitácora.
    /// </summary>
    [RequierePrivilegio(Permisos.CarroCambiarEstatus, Permisos.ViajeRegistrar)]
    [HttpPost]
    public async Task<ActionResult<CambioEstatusCarroDto>> Cambiar(CambiarEstatusCarroRequest request, CancellationToken cancellationToken)
    {
        var command = new CambiarEstatusCarroCommand(
            request.CarroId,
            request.EmpleadoId,
            request.EstatusNuevo!.Value,
            request.Motivo,
            request.ViajeAfectadoId,
            request.FechaCambio,
            request.TipoMotivo);

        var cambio = await _sender.Send(command, cancellationToken);

        return CreatedAtAction(nameof(GetById), new { cambioEstatusCarroId = cambio.CambioEstatusCarroId }, cambio);
    }

    /// <summary>
    /// HU-EST-02: agrega la foto de la falla o del daño (multipart/form-data, campo "archivo": jpg, png o webp, máx. 5 MB).
    /// </summary>
    [RequierePrivilegio(Permisos.CarroCambiarEstatus, Permisos.ViajeRegistrar)]
    [HttpPut("{cambioEstatusCarroId:guid}/foto-evidencia")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(FormFileExtensions.LimiteCuerpoBytes)]
    [RequestFormLimits(MultipartBodyLengthLimit = FormFileExtensions.LimiteCuerpoBytes)]
    public async Task<ActionResult<CambioEstatusCarroDto>> SubirFotoEvidencia(Guid cambioEstatusCarroId, [FromForm] SubirArchivoRequest request, CancellationToken cancellationToken)
    {
        await using var contenido = request.Archivo!.OpenReadStream();

        var cambio = await _sender.Send(
            new SubirFotoEvidenciaCommand(cambioEstatusCarroId, request.Archivo.ToArchivoEntrada(contenido)),
            cancellationToken);

        return Ok(cambio);
    }
}
