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

    /// <summary>Puede corregir viajes y cargas ya cerrados (junto con el Confirmador).</summary>
    public const string GestorActivos = "Gestor de activos";

    /// <summary>Personal interno de la desarrolladora; sin empresa en el token.</summary>
    public const string Administrador = "Administrador";

    /// <summary>Consulta reportes y el módulo de auditoría y trazabilidad.</summary>
    public const string ConsultorReportes = "Consultor/Reportes";
}
