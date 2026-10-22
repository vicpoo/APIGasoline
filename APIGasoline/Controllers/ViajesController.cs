using APIGasoline.Application.Features.Viajes.Commands;
using APIGasoline.Application.Features.Viajes.Common;
using APIGasoline.Application.Features.Viajes.DTOs;
using APIGasoline.Application.Features.Viajes.Queries;
using APIGasoline.Contracts.Archivos;
using APIGasoline.Contracts.Viajes;
using APIGasoline.Domain.Entities;
using APIGasoline.Domain.Enums;
using APIGasoline.Extensions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using APIGasoline.Application.Common.Security;
using APIGasoline.Authorization;

namespace APIGasoline.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ViajesController : ControllerBase
{
    private readonly ISender _sender;

    public ViajesController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Lista del más reciente al más antiguo.
    /// Filtros opcionales: ?empresaId=, ?carroId=, ?empleadoOperadorId=, ?solicitudReservaId=, ?estado=enCurso
    /// </summary>
    [RequierePrivilegio(Permisos.ViajeVerPropios, Permisos.ViajeVerTodos)]
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ViajeDto>>> GetAll(
        [FromQuery] Guid? empresaId,
        [FromQuery] Guid? carroId,
        [FromQuery] Guid? empleadoOperadorId,
        [FromQuery] Guid? solicitudReservaId,
        [FromQuery] EstadoViaje? estado,
        CancellationToken cancellationToken)
    {
        var viajes = await _sender.Send(
            new GetViajesQuery(empresaId, carroId, empleadoOperadorId, solicitudReservaId, estado), cancellationToken);

        return Ok(viajes);
    }

    [RequierePrivilegio(Permisos.ViajeVerDetalle, Permisos.ViajeRegistrar)]
    [HttpGet("{viajeId:guid}")]
    public async Task<ActionResult<ViajeDto>> GetById(Guid viajeId, CancellationToken cancellationToken)
    {
        var viaje = await _sender.Send(new GetViajeByIdQuery(viajeId), cancellationToken);
        return Ok(viaje);
    }

    /// <summary>
    /// Inicia un viaje (en línea).
    /// </summary>
    [RequierePrivilegio(Permisos.ViajeRegistrar)]
    [HttpPost]
    public async Task<ActionResult<ViajeDto>> Iniciar(IniciarViajeRequest request, CancellationToken cancellationToken)
    {
        var command = new IniciarViajeCommand(
            request.EmpresaId,
            request.CarroId,
            request.SolicitudReservaId,
            request.EmpleadoOperadorId,
            request.FechaInicio!.Value,
            request.UbicacionInicioLat,
            request.UbicacionInicioLng,
            request.KilometrajeInicial,
            request.CentroCostoId);

        var viaje = await _sender.Send(command, cancellationToken);

        return CreatedAtAction(nameof(GetById), new { viajeId = viaje.ViajeId }, viaje);
    }

    /// <summary>Pone el viaje en pausa (el GPS se detiene; los puntos dentro de la pausa se descartan).</summary>
    [RequierePrivilegio(Permisos.ViajeRegistrar)]
    [HttpPost("{viajeId:guid}/pausar")]
    public async Task<ActionResult<ViajeDto>> Pausar(
        Guid viajeId,
        [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] PausarViajeRequest? request,
        CancellationToken cancellationToken)
    {
        // El cuerpo es opcional: sin datos, la pausa empieza a la hora del servidor
        var command = new PausarViajeCommand(viajeId, request?.Fecha, request?.Motivo, request?.UbicacionLat, request?.UbicacionLng);
        return Ok(await _sender.Send(command, cancellationToken));
    }

    /// <summary>Reanuda un viaje en pausa.</summary>
    [RequierePrivilegio(Permisos.ViajeRegistrar)]
    [HttpPost("{viajeId:guid}/reanudar")]
    public async Task<ActionResult<ViajeDto>> Reanudar(
        Guid viajeId,
        [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] ReanudarViajeRequest? request,
        CancellationToken cancellationToken)
        => Ok(await _sender.Send(new ReanudarViajeCommand(viajeId, request?.Fecha), cancellationToken));

    [RequierePrivilegio(Permisos.ViajeRegistrar)]
    [HttpPost("{viajeId:guid}/finalizar")]
    public async Task<ActionResult<ViajeDto>> Finalizar(Guid viajeId, FinalizarViajeRequest request, CancellationToken cancellationToken)
    {
        var command = new FinalizarViajeCommand(
            viajeId,
            request.FechaFin!.Value,
            request.UbicacionFinLat,
            request.UbicacionFinLng,
            request.KilometrajeFinal!.Value);

        return Ok(await _sender.Send(command, cancellationToken));
    }

    [RequierePrivilegio(Permisos.ViajeRegistrar, Permisos.ViajeCorregirCerrado)]
    [HttpPost("{viajeId:guid}/interrumpir")]
    public async Task<ActionResult<ViajeDto>> Interrumpir(Guid viajeId, InterrumpirViajeRequest request, CancellationToken cancellationToken)
    {
        var command = new InterrumpirViajeCommand(
            viajeId,
            request.Motivo,
            request.FechaFin,
            request.UbicacionFinLat,
            request.UbicacionFinLng,
            request.KilometrajeFinal);

        return Ok(await _sender.Send(command, cancellationToken));
    }

    /// <summary>
    /// Sube un viaje registrado sin conexión (la BD guarda la hora de llegada en fecha_recepcion).
    /// </summary>
    [RequierePrivilegio(Permisos.ViajeRegistrar)]
    [HttpPost("sincronizar")]
    public async Task<ActionResult<ViajeDto>> Sincronizar(SincronizarViajeRequest request, CancellationToken cancellationToken)
    {
        var command = new SincronizarViajeCommand(
            request.EmpresaId,
            request.CarroId,
            request.SolicitudReservaId,
            request.EmpleadoOperadorId,
            request.FechaInicio!.Value,
            request.UbicacionInicioLat,
            request.UbicacionInicioLng,
            request.KilometrajeInicial,
            request.Estado,
            request.FechaFin,
            request.UbicacionFinLat,
            request.UbicacionFinLng,
            request.KilometrajeFinal,
            request.MotivoInterrupcion,
            request.CentroCostoId,
            request.Pausas?
                .Select(p => new DatosPausa(p.FechaInicio!.Value, p.FechaFin, p.Motivo, p.UbicacionLat, p.UbicacionLng))
                .ToList());

        var viaje = await _sender.Send(command, cancellationToken);

        return CreatedAtAction(nameof(GetById), new { viajeId = viaje.ViajeId }, viaje);
    }

    /// <summary>
    /// Sube o reemplaza la foto del tablero (odómetro y gasolina) al iniciar o terminar el viaje.
    /// Ruta: /foto-tablero/inicial o /foto-tablero/final. multipart/form-data, campo "archivo": jpg, png o webp, máx. 5 MB.
    /// La foto anterior no se borra (es evidencia).
    /// </summary>
    [RequierePrivilegio(Permisos.ViajeRegistrar)]
    [HttpPut("{viajeId:guid}/foto-tablero/{momento}")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(FormFileExtensions.LimiteCuerpoBytes)]
    [RequestFormLimits(MultipartBodyLengthLimit = FormFileExtensions.LimiteCuerpoBytes)]
    public async Task<ActionResult<ViajeDto>> SubirFotoTablero(Guid viajeId, string momento, [FromForm] SubirArchivoRequest request, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<MomentoTablero>(momento, ignoreCase: true, out var momentoTablero) || !Enum.IsDefined(momentoTablero))
            return BadRequest(new ProblemDetails { Title = "Solicitud inválida", Detail = "El momento debe ser 'inicial' o 'final'.", Status = StatusCodes.Status400BadRequest });

        await using var contenido = request.Archivo!.OpenReadStream();

        var viaje = await _sender.Send(
            new SubirFotoTableroCommand(viajeId, momentoTablero, request.Archivo.ToArchivoEntrada(contenido)),
            cancellationToken);

        return Ok(viaje);
    }

    /// <summary>
    /// Corrige el kilometraje capturado por error (con motivo). Requiere token.
    /// El viaje queda marcado como corregido y el valor anterior se conserva en el historial de auditoría.
    /// </summary>
    [RequierePrivilegio(Permisos.ViajeRegistrar, Permisos.ViajeCorregirCerrado)]
    [HttpPost("{viajeId:guid}/corregir-kilometraje")]
    public async Task<ActionResult<ViajeDto>> CorregirKilometraje(Guid viajeId, CorregirKilometrajeViajeRequest request, CancellationToken cancellationToken)
    {
        var command = new CorregirKilometrajeViajeCommand(viajeId, request.KilometrajeInicial!.Value, request.KilometrajeFinal, request.Motivo);
        return Ok(await _sender.Send(command, cancellationToken));
    }

    [RequierePrivilegio(Permisos.ViajeEliminar)]
    [HttpDelete("{viajeId:guid}")]
    public async Task<IActionResult> Delete(Guid viajeId, CancellationToken cancellationToken)
    {
        await _sender.Send(new DeleteViajeCommand(viajeId), cancellationToken);
        return NoContent();
    }
}
