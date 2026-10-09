using APIGasoline.Application.Features.Dispositivos.Commands;
using APIGasoline.Application.Features.Dispositivos.DTOs;
using APIGasoline.Application.Features.Dispositivos.Queries;
using APIGasoline.Contracts.Dispositivos;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace APIGasoline.Controllers;

/// <summary>
/// Celulares que reciben notificaciones push (token de Firebase Cloud Messaging).
/// </summary>
[ApiController]
public class DispositivosController : ControllerBase
{
    private readonly ISender _sender;

    public DispositivosController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Registra (o reactiva) el token del celular. La app lo llama al iniciar sesión
    /// y cada vez que Firebase le entrega un token nuevo.
    /// </summary>
    [HttpPost("api/dispositivos")]
    public async Task<ActionResult<DispositivoDto>> Registrar(RegistrarDispositivoRequest request, CancellationToken cancellationToken)
    {
        var dispositivo = await _sender.Send(
            new RegistrarDispositivoCommand(request.EmpleadoId, request.TokenFcm, request.Plataforma!.Value),
            cancellationToken);

        return Ok(dispositivo);
    }

    /// <summary>Al cerrar sesión: el celular deja de recibir push.</summary>
    [HttpDelete("api/dispositivos/{dispositivoEmpleadoId:guid}")]
    public async Task<IActionResult> Desactivar(Guid dispositivoEmpleadoId, CancellationToken cancellationToken)
    {
        await _sender.Send(new DesactivarDispositivoCommand(dispositivoEmpleadoId), cancellationToken);
        return NoContent();
    }

    [HttpGet("api/empleados/{empleadoId:guid}/dispositivos")]
    public async Task<ActionResult<IReadOnlyList<DispositivoDto>>> GetByEmpleado(Guid empleadoId, CancellationToken cancellationToken)
    {
        var dispositivos = await _sender.Send(new GetDispositivosByEmpleadoQuery(empleadoId), cancellationToken);
        return Ok(dispositivos);
    }
}
