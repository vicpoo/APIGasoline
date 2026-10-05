using APIGasoline.Domain.Enums;

namespace APIGasoline.Application.Features.Carros.DTOs;

public record CarroDto(
    int CarroId,
    int EmpresaId,
    string Marca,
    string Modelo,
    int? Anio,
    string? Color,
    string Placas,
    string? FotoVehiculo,
    decimal? CapacidadTanque,
    decimal KilometrajeActual,
    EstadoCarro Estado,
    DateTime? FechaAlta);
