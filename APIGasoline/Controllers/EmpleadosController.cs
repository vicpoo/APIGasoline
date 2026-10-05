using APIGasoline.Application.Features.Empleados.Commands;
using APIGasoline.Application.Features.Empleados.DTOs;
using APIGasoline.Application.Features.Empleados.Queries;
using APIGasoline.Contracts.Empleados;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace APIGasoline.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmpleadosController : ControllerBase
{
    private readonly ISender _sender;

    public EmpleadosController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Lista los empleados no eliminados. Filtro opcional: ?empresaId=1
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<EmpleadoDto>>> GetAll([FromQuery] int? empresaId, CancellationToken cancellationToken)
    {
        var empleados = await _sender.Send(new GetEmpleadosQuery(empresaId), cancellationToken);
        return Ok(empleados);
    }

    [HttpGet("{empleadoId:int}")]
    public async Task<ActionResult<EmpleadoDto>> GetById(int empleadoId, CancellationToken cancellationToken)
    {
        var empleado = await _sender.Send(new GetEmpleadoByIdQuery(empleadoId), cancellationToken);
        return Ok(empleado);
    }

    /// <summary>
    /// Obtiene el empleado ligado a un usuario del servicio de seguridad.
    /// </summary>
    [HttpGet("por-usuario/{usuarioId:int}")]
    public async Task<ActionResult<EmpleadoDto>> GetByUsuarioId(int usuarioId, CancellationToken cancellationToken)
    {
        var empleado = await _sender.Send(new GetEmpleadoByUsuarioIdQuery(usuarioId), cancellationToken);
        return Ok(empleado);
    }

    [HttpPost]
    public async Task<ActionResult<EmpleadoDto>> Create(CreateEmpleadoRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateEmpleadoCommand(request.EmpresaId, request.UsuarioId, request.NombreCompleto, request.FotoPerfil, request.FotoLicencia);
        var empleado = await _sender.Send(command, cancellationToken);

        return CreatedAtAction(nameof(GetById), new { empleadoId = empleado.EmpleadoId }, empleado);
    }

    [HttpPut("{empleadoId:int}")]
    public async Task<ActionResult<EmpleadoDto>> Update(int empleadoId, UpdateEmpleadoRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateEmpleadoCommand(empleadoId, request.EmpresaId, request.UsuarioId, request.NombreCompleto, request.FotoPerfil, request.FotoLicencia);
        var empleado = await _sender.Send(command, cancellationToken);

        return Ok(empleado);
    }

    /// <summary>
    /// Soft delete: el empleado queda marcado como eliminado.
    /// </summary>
    [HttpDelete("{empleadoId:int}")]
    public async Task<IActionResult> Delete(int empleadoId, CancellationToken cancellationToken)
    {
        await _sender.Send(new DeleteEmpleadoCommand(empleadoId), cancellationToken);
        return NoContent();
    }
}
