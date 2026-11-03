namespace APIGasoline.Extensions;

/// <summary>
/// Normaliza las fechas que llegan por multipart/form-data ([FromForm]).
/// </summary>
/// <remarks>
/// Desde ASP.NET Core 5 el model binder de formularios convierte a UTC (Kind = Utc) las fechas que
/// traen zona horaria ("2026-11-03T12:30:00-06:00" llega como 18:30 UTC), mientras que el JSON
/// (System.Text.Json) las convierte a la hora local del servidor. La API trabaja en hora local
/// (DateTimeProvider.Now = DateTime.Now y TZ=America/Mexico_City), así que sin esta conversión una carga
/// se comparaba 6 horas "adelantada" contra el viaje y se rechazaba por "fuera del periodo del viaje".
/// </remarks>
public static class FechaFormularioExtensions
{
    /// <summary>Si la fecha viene en UTC la pasa a la hora local del servidor; si no, la deja igual.</summary>
    public static DateTime AHoraLocal(this DateTime fecha)
        => fecha.Kind == DateTimeKind.Utc ? fecha.ToLocalTime() : fecha;
}
