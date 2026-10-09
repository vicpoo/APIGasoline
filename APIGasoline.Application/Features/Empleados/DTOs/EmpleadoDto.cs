namespace APIGasoline.Application.Features.Empleados.DTOs;

public record EmpleadoDto(
    Guid EmpleadoId,
    Guid EmpresaId,
    Guid UsuarioId,
    string NombreCompleto,
    /// <summary>Llaves de los archivos en el almacenamiento (lo que se guarda en BD).</summary>
    string? FotoPerfil,
    string? FotoLicencia,
    /// <summary>URLs temporales para ver los archivos (las llena el handler, no AutoMapper).</summary>
    string? FotoPerfilUrl,
    string? FotoLicenciaUrl,
    bool TieneLicencia,
    DateTime? FechaAlta);
