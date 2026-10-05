using APIGasoline.Application.Features.Empresas.Commands;
using APIGasoline.Application.Features.Empresas.DTOs;
using APIGasoline.Application.Features.Empresas.Queries;
using APIGasoline.Contracts.Empresas;
using MediatR;
using Microsoft.AspNetCore.Mvc;

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
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<EmpresaDto>>> GetAll(
        [FromQuery] int? corporativoId,
        [FromQuery] int? divisionId,
        CancellationToken cancellationToken)
    {
        var empresas = await _sender.Send(new GetEmpresasQuery(corporativoId, divisionId), cancellationToken);
        return Ok(empresas);
    }

    [HttpGet("{empresaId:int}")]
    public async Task<ActionResult<EmpresaDto>> GetById(int empresaId, CancellationToken cancellationToken)
    {
        var empresa = await _sender.Send(new GetEmpresaByIdQuery(empresaId), cancellationToken);
        return Ok(empresa);
    }

    [HttpPost]
    public async Task<ActionResult<EmpresaDto>> Create(CreateEmpresaRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateEmpresaCommand(request.Nombre, request.CorporativoId, request.DivisionId);
        var empresa = await _sender.Send(command, cancellationToken);

        return CreatedAtAction(nameof(GetById), new { empresaId = empresa.EmpresaId }, empresa);
    }

    [HttpPut("{empresaId:int}")]
    public async Task<ActionResult<EmpresaDto>> Update(int empresaId, UpdateEmpresaRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateEmpresaCommand(empresaId, request.Nombre, request.CorporativoId, request.DivisionId);
        var empresa = await _sender.Send(command, cancellationToken);

        return Ok(empresa);
    }

    /// <summary>
    /// Soft delete: la empresa queda marcada como eliminada.
    /// </summary>
    [HttpDelete("{empresaId:int}")]
    public async Task<IActionResult> Delete(int empresaId, CancellationToken cancellationToken)
    {
        await _sender.Send(new DeleteEmpresaCommand(empresaId), cancellationToken);
        return NoContent();
    }
}
