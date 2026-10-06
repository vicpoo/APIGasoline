using APIGasoline.Application.Features.Carros.Commands;
using APIGasoline.Application.Features.Carros.DTOs;
using APIGasoline.Application.Features.Carros.Queries;
using APIGasoline.Contracts.Archivos;
using APIGasoline.Contracts.Carros;
using APIGasoline.Domain.Enums;
using APIGasoline.Extensions;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace APIGasoline.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CarrosController : ControllerBase
{
    private readonly ISender _sender;

    public CarrosController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Lista los carros. Filtros opcionales: ?empresaId=1 y/o ?estado=disponible
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CarroDto>>> GetAll(
        [FromQuery] int? empresaId,
        [FromQuery] EstadoCarro? estado,
        CancellationToken cancellationToken)
    {
        var carros = await _sender.Send(new GetCarrosQuery(empresaId, estado), cancellationToken);
        return Ok(carros);
    }

    [HttpGet("{carroId:int}")]
    public async Task<ActionResult<CarroDto>> GetById(int carroId, CancellationToken cancellationToken)
    {
        var carro = await _sender.Send(new GetCarroByIdQuery(carroId), cancellationToken);
        return Ok(carro);
    }

    [HttpPost]
    public async Task<ActionResult<CarroDto>> Create(CreateCarroRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateCarroCommand(
            request.EmpresaId,
            request.Marca,
            request.Modelo,
            request.Anio,
            request.Color,
            request.Placas,
            request.CapacidadTanque,
            request.KilometrajeActual,
            request.Estado);

        var carro = await _sender.Send(command, cancellationToken);

        return CreatedAtAction(nameof(GetById), new { carroId = carro.CarroId }, carro);
    }

    [HttpPut("{carroId:int}")]
    public async Task<ActionResult<CarroDto>> Update(int carroId, UpdateCarroRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateCarroCommand(
            carroId,
            request.EmpresaId,
            request.Marca,
            request.Modelo,
            request.Anio,
            request.Color,
            request.Placas,
            request.CapacidadTanque,
            request.KilometrajeActual);

        var carro = await _sender.Send(command, cancellationToken);

        return Ok(carro);
    }

    /// <summary>
    /// Sube o reemplaza la foto del vehículo (multipart/form-data, campo "archivo": jpg, png o webp, máx. 5 MB).
    /// </summary>
    [HttpPut("{carroId:int}/foto")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(FormFileExtensions.LimiteCuerpoBytes)]
    [RequestFormLimits(MultipartBodyLengthLimit = FormFileExtensions.LimiteCuerpoBytes)]
    public async Task<ActionResult<CarroDto>> SubirFoto(int carroId, [FromForm] SubirArchivoRequest request, CancellationToken cancellationToken)
    {
        await using var contenido = request.Archivo!.OpenReadStream();

        var carro = await _sender.Send(
            new SubirFotoVehiculoCommand(carroId, request.Archivo.ToArchivoEntrada(contenido)),
            cancellationToken);

        return Ok(carro);
    }

    [HttpDelete("{carroId:int}/foto")]
    public async Task<IActionResult> EliminarFoto(int carroId, CancellationToken cancellationToken)
    {
        await _sender.Send(new EliminarFotoVehiculoCommand(carroId), cancellationToken);
        return NoContent();
    }

    [HttpDelete("{carroId:int}")]
    public async Task<IActionResult> Delete(int carroId, CancellationToken cancellationToken)
    {
        await _sender.Send(new DeleteCarroCommand(carroId), cancellationToken);
        return NoContent();
    }
}
