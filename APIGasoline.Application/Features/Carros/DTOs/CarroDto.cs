using APIGasoline.Domain.Enums;

namespace APIGasoline.Application.Features.Carros.DTOs;

public record CarroDto(
    Guid CarroId,
    Guid EmpresaId,
    string Marca,
    string Modelo,
    int? Anio,
    string? Color,
    string Placas,
    /// <summary>Llave del archivo en el almacenamiento (lo que se guarda en BD).</summary>
    string? FotoVehiculo,
    /// <summary>URL temporal para ver la foto (la llena el handler, no AutoMapper).</summary>
    string? FotoVehiculoUrl,
    decimal? CapacidadTanque,
    decimal KilometrajeActual,
    EstadoCarro Estado,
    /// <summary>Baja del carro (HU-AST-03).</summary>
    bool DadoDeBaja,
    DateTime? FechaBaja,
    string? MotivoBaja,
    DateTime? FechaAlta);
