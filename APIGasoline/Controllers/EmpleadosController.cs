using APIGasoline.Application.Features.Empleados.Commands;
using APIGasoline.Application.Features.Empleados.Common;
using APIGasoline.Application.Features.Empleados.DTOs;
using APIGasoline.Application.Features.Empleados.Queries;
using APIGasoline.Contracts.Archivos;
using APIGasoline.Contracts.Empleados;
using APIGasoline.Extensions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using APIGasoline.Application.Common.Security;
using APIGasoline.Authorization;

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
    [RequierePrivilegio(Permisos.EmpleadoVer, Permisos.EmpleadoGestionar, Permisos.ViajeVerTodos, Permisos.SolicitudAprobar)]
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<EmpleadoDto>>> GetAll([FromQuery] Guid? empresaId, CancellationToken cancellationToken)
    {
        var empleados = await _sender.Send(new GetEmpleadosQuery(empresaId), cancellationToken);
        return Ok(empleados);
    }

    [HttpGet("{empleadoId:guid}")]
    public async Task<ActionResult<EmpleadoDto>> GetById(Guid empleadoId, CancellationToken cancellationToken)
    {
        var empleado = await _sender.Send(new GetEmpleadoByIdQuery(empleadoId), cancellationToken);
        return Ok(empleado);
    }

    /// <summary>
    /// Obtiene el empleado ligado a un usuario del servicio de seguridad.
    /// </summary>
    [HttpGet("por-usuario/{usuarioId:guid}")]
    public async Task<ActionResult<EmpleadoDto>> GetByUsuarioId(Guid usuarioId, CancellationToken cancellationToken)
    {
        var empleado = await _sender.Send(new GetEmpleadoByUsuarioIdQuery(usuarioId), cancellationToken);
        return Ok(empleado);
    }

    [RequierePrivilegio(Permisos.EmpleadoGestionar)]
    [HttpPost]
    public async Task<ActionResult<EmpleadoDto>> Create(CreateEmpleadoRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateEmpleadoCommand(request.EmpresaId, request.UsuarioId, request.NombreCompleto);
        var empleado = await _sender.Send(command, cancellationToken);

        return CreatedAtAction(nameof(GetById), new { empleadoId = empleado.EmpleadoId }, empleado);
    }

    [HttpPut("{empleadoId:guid}")]
    public async Task<ActionResult<EmpleadoDto>> Update(Guid empleadoId, UpdateEmpleadoRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateEmpleadoCommand(empleadoId, request.EmpresaId, request.UsuarioId, request.NombreCompleto);
        var empleado = await _sender.Send(command, cancellationToken);

        return Ok(empleado);
    }

    /// <summary>
    /// Sube o reemplaza la foto de perfil (multipart/form-data, campo "archivo": jpg, png o webp, máx. 5 MB).
    /// </summary>
    [HttpPut("{empleadoId:guid}/foto-perfil")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(FormFileExtensions.LimiteCuerpoBytes)]
    [RequestFormLimits(MultipartBodyLengthLimit = FormFileExtensions.LimiteCuerpoBytes)]
    public Task<ActionResult<EmpleadoDto>> SubirFotoPerfil(Guid empleadoId, [FromForm] SubirArchivoRequest request, CancellationToken cancellationToken)
        => SubirArchivoAsync(empleadoId, TipoFotoEmpleado.Perfil, request, cancellationToken);

    /// <summary>
    /// Sube o reemplaza la licencia (multipart/form-data, campo "archivo": jpg, png, webp o pdf, máx. 5 MB).
    /// </summary>
    [HttpPut("{empleadoId:guid}/foto-licencia")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(FormFileExtensions.LimiteCuerpoBytes)]
    [RequestFormLimits(MultipartBodyLengthLimit = FormFileExtensions.LimiteCuerpoBytes)]
    public Task<ActionResult<EmpleadoDto>> SubirFotoLicencia(Guid empleadoId, [FromForm] SubirArchivoRequest request, CancellationToken cancellationToken)
        => SubirArchivoAsync(empleadoId, TipoFotoEmpleado.Licencia, request, cancellationToken);

    [HttpDelete("{empleadoId:guid}/foto-perfil")]
    public async Task<IActionResult> EliminarFotoPerfil(Guid empleadoId, CancellationToken cancellationToken)
    {
        await _sender.Send(new EliminarFotoEmpleadoCommand(empleadoId, TipoFotoEmpleado.Perfil), cancellationToken);
        return NoContent();
    }

    [HttpDelete("{empleadoId:guid}/foto-licencia")]
    public async Task<IActionResult> EliminarFotoLicencia(Guid empleadoId, CancellationToken cancellationToken)
    {
        await _sender.Send(new EliminarFotoEmpleadoCommand(empleadoId, TipoFotoEmpleado.Licencia), cancellationToken);
        return NoContent();
    }

    private async Task<ActionResult<EmpleadoDto>> SubirArchivoAsync(
        Guid empleadoId,
        TipoFotoEmpleado tipo,
        SubirArchivoRequest request,
        CancellationToken cancellationToken)
    {
        await using var contenido = request.Archivo!.OpenReadStream();

        var empleado = await _sender.Send(
            new SubirFotoEmpleadoCommand(empleadoId, tipo, request.Archivo.ToArchivoEntrada(contenido)),
            cancellationToken);

        return Ok(empleado);
    }

    /// <summary>
    /// Soft delete: el empleado queda marcado como eliminado.
    /// </summary>
    [RequierePrivilegio(Permisos.EmpleadoGestionar)]
    [HttpDelete("{empleadoId:guid}")]
    public async Task<IActionResult> Delete(Guid empleadoId, CancellationToken cancellationToken)
    {
        await _sender.Send(new DeleteEmpleadoCommand(empleadoId), cancellationToken);
        return NoContent();
    }
}
