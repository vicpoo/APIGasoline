using APIGasoline.Domain.Enums;
using APIGasoline.Domain.Interfaces;

namespace APIGasoline.Application.Common.Sesiones;

/// <summary>
/// Cierra sesiones en bloque (cambio de contraseña, usuario desactivado, empleado dado de baja...).
/// Solo marca las entidades: el SaveChanges lo hace el handler que la llama, así el cierre queda en la
/// MISMA transacción que el cambio que lo provocó.
///
/// Nota: el JWT de acceso que ya tenga el dispositivo sigue sirviendo hasta que expire
/// (Jwt:ExpiracionMinutos, 15 min); lo que ya no puede es renovarse.
/// </summary>
public static class RevocacionSesiones
{
    /// <summary>Marca como cerradas las sesiones activas del usuario. Devuelve cuántas cerró.</summary>
    public static async Task<int> RevocarDeUsuarioAsync(
        this IUnitOfWork unitOfWork,
        Guid usuarioId,
        MotivoRevocacionSesion motivo,
        DateTime ahora,
        Guid? revocadaPorUsuarioId = null,
        Guid? exceptoSesionId = null,
        CancellationToken cancellationToken = default)
    {
        var sesiones = await unitOfWork.Sesiones.GetActivasByUsuarioAsync(usuarioId, ahora, cancellationToken);
        var cerradas = 0;

        foreach (var sesion in sesiones)
        {
            if (sesion.SesionUsuarioId == exceptoSesionId)
                continue;

            sesion.Revocar(motivo, ahora, revocadaPorUsuarioId);
            cerradas++;
        }

        return cerradas;
    }

    /// <summary>Marca como cerradas las sesiones activas iniciadas por empleados de la empresa.</summary>
    public static async Task<int> RevocarDeEmpresaAsync(
        this IUnitOfWork unitOfWork,
        Guid empresaId,
        MotivoRevocacionSesion motivo,
        DateTime ahora,
        Guid? revocadaPorUsuarioId = null,
        CancellationToken cancellationToken = default)
    {
        var sesiones = await unitOfWork.Sesiones.GetActivasByEmpresaAsync(empresaId, ahora, cancellationToken);

        foreach (var sesion in sesiones)
            sesion.Revocar(motivo, ahora, revocadaPorUsuarioId);

        return sesiones.Count;
    }
}
