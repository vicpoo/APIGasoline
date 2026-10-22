using APIGasoline.Application.Features.Empresas.Commands;
using APIGasoline.Application.Features.Empresas.DTOs;
using APIGasoline.Application.Features.Empresas.Queries;
using APIGasoline.Contracts.Empresas;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using APIGasoline.Application.Common.Security;
using APIGasoline.Authorization;

namespace APIGasoline.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmpresasController : ControllerBase
{
    private readonly ISender _sender;

    public EmpresasController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Lista las empresas no eliminadas. Filtros opcionales: ?corporativoId=1 y/o ?divisionId=2
    /// </summary>
    [RequierePrivilegio(Permisos.EmpresaVer, Permisos.EmpresaGestionar)]
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<EmpresaDto>>> GetAll(
        [FromQuery] Guid? corporativoId,
        [FromQuery] Guid? divisionId,
        CancellationToken cancellationToken)
    {
        var empresas = await _sender.Send(new GetEmpresasQuery(corporativoId, divisionId), cancellationToken);
        return Ok(empresas);
    }

    [RequierePrivilegio(Permisos.EmpresaVer, Permisos.EmpresaGestionar)]
    [HttpGet("{empresaId:guid}")]
    public async Task<ActionResult<EmpresaDto>> GetById(Guid empresaId, CancellationToken cancellationToken)
    {
        var empresa = await _sender.Send(new GetEmpresaByIdQuery(empresaId), cancellationToken);
        return Ok(empresa);
    }

    [RequierePrivilegio(Permisos.EmpresaGestionar)]
    [HttpPost]
    public async Task<ActionResult<EmpresaDto>> Create(CreateEmpresaRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateEmpresaCommand(request.Nombre, request.CorporativoId, request.DivisionId);
        var empresa = await _sender.Send(command, cancellationToken);

        return CreatedAtAction(nameof(GetById), new { empresaId = empresa.EmpresaId }, empresa);
    }

    [RequierePrivilegio(Permisos.EmpresaEditar, Permisos.EmpresaGestionar)]
    [HttpPut("{empresaId:guid}")]
    public async Task<ActionResult<EmpresaDto>> Update(Guid empresaId, UpdateEmpresaRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateEmpresaCommand(empresaId, request.Nombre, request.CorporativoId, request.DivisionId, request.Activo);
        var empresa = await _sender.Send(command, cancellationToken);

        return Ok(empresa);
    }

    /// <summary>
    /// Soft delete: la empresa queda marcada como eliminada.
    /// </summary>
    [RequierePrivilegio(Permisos.EmpresaGestionar)]
    [HttpDelete("{empresaId:guid}")]
    public async Task<IActionResult> Delete(Guid empresaId, CancellationToken cancellationToken)
    {
        await _sender.Send(new DeleteEmpresaCommand(empresaId), cancellationToken);
        return NoContent();
    }
}
