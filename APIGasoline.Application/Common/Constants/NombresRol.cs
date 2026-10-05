namespace APIGasoline.Application.Common.Constants;

/// <summary>
/// Nombres de roles (tabla rol) que la lógica de negocio necesita conocer.
/// </summary>
public static class NombresRol
{
    /// <summary>
    /// Los empleados de la empresa cuyo usuario tiene este rol (activo) reciben
    /// las notificaciones de nuevas solicitudes de reserva.
    /// </summary>
    public const string Confirmador = "Confirmador";
}
