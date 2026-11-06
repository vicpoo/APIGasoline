namespace APIGasoline.Application.Common.Sesiones;

/// <summary>
/// Sección "Sesiones" de appsettings (o variables de entorno Sesiones__RefreshDias, etc.).
/// La duración del token de ACCESO (JWT) sigue en Jwt:ExpiracionMinutos; aquí va la del refresh token.
/// </summary>
public class SesionesSettings
{
    public const string SectionName = "Sesiones";

    /// <summary>
    /// Días que dura la sesión sin usarse. Cada renovación la vuelve a extender
    /// (expiración deslizante): si el usuario abre la app al menos una vez en este plazo, no vuelve a iniciar sesión.
    /// </summary>
    public int RefreshDias { get; set; } = 30;

    /// <summary>
    /// Tope absoluto desde el inicio de sesión: aunque se use todos los días, después de este plazo
    /// hay que volver a escribir la contraseña. Debe ser mayor o igual a <see cref="RefreshDias"/>.
    /// </summary>
    public int RefreshMaxDias { get; set; } = 90;

    /// <summary>
    /// Si un token ya reemplazado se vuelve a usar dentro de este plazo después de la rotación,
    /// se asume que fueron dos peticiones simultáneas del mismo celular (no un robo) y la sesión NO se cierra.
    /// </summary>
    public int GraciaReusoSegundos { get; set; } = 30;

    /// <summary>Máximo de sesiones abiertas por usuario. Al pasarse, se cierra la que lleva más tiempo sin usarse.</summary>
    public int MaxSesionesPorUsuario { get; set; } = 10;

    /// <summary>Permite apagar el worker que borra sesiones viejas.</summary>
    public bool LimpiezaHabilitada { get; set; } = true;

    /// <summary>Cada cuántas horas corre la limpieza.</summary>
    public int LimpiezaIntervaloHoras { get; set; } = 6;

    /// <summary>Días que se conservan las sesiones expiradas o cerradas (para consulta) antes de borrarlas.</summary>
    public int LimpiezaRetencionDias { get; set; } = 30;

    public TimeSpan DuracionRefresh => TimeSpan.FromDays(RefreshDias);

    public TimeSpan DuracionMaxima => TimeSpan.FromDays(RefreshMaxDias);

    public TimeSpan GraciaReuso => TimeSpan.FromSeconds(GraciaReusoSegundos);

    public void Validar()
    {
        if (RefreshDias is < 1 or > 365)
            throw new InvalidOperationException("Sesiones:RefreshDias debe estar entre 1 y 365.");

        if (RefreshMaxDias < RefreshDias || RefreshMaxDias > 730)
            throw new InvalidOperationException("Sesiones:RefreshMaxDias debe ser mayor o igual a RefreshDias y no mayor a 730.");

        if (GraciaReusoSegundos is < 0 or > 300)
            throw new InvalidOperationException("Sesiones:GraciaReusoSegundos debe estar entre 0 y 300.");

        if (MaxSesionesPorUsuario is < 1 or > 100)
            throw new InvalidOperationException("Sesiones:MaxSesionesPorUsuario debe estar entre 1 y 100.");

        if (LimpiezaIntervaloHoras is < 1 or > 168)
            throw new InvalidOperationException("Sesiones:LimpiezaIntervaloHoras debe estar entre 1 y 168.");

        if (LimpiezaRetencionDias is < 0 or > 365)
            throw new InvalidOperationException("Sesiones:LimpiezaRetencionDias debe estar entre 0 y 365.");
    }
}
