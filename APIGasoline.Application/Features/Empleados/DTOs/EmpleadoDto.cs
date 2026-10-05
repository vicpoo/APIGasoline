namespace APIGasoline.Application.Features.Empleados.DTOs;

public record EmpleadoDto(
    int EmpleadoId,
    int EmpresaId,
    int UsuarioId,
    string NombreCompleto,
    string? FotoPerfil,
    string? FotoLicencia,
    bool TieneLicencia,
    DateTime? FechaAlta);
