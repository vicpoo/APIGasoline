namespace APIGasoline.Application.Common.Interfaces;

/// <summary>
/// Privilegios efectivos de un usuario (la implementación vive en Infrastructure y usa caché).
/// Misma regla que el login: privilegios ACTIVOS de los roles ACTIVOS del usuario que sean
/// globales o de su empresa. Un usuario inactivo no tiene privilegios.
/// </summary>
public interface IConsultaPrivilegios
{
    Task<IReadOnlySet<string>> ObtenerAsync(Guid usuarioId, Guid? empresaId, CancellationToken cancellationToken = default);
}
