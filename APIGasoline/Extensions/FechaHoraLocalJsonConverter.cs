using System.Text.Json;
using System.Text.Json.Serialization;

namespace APIGasoline.Extensions;

/// <summary>
/// A001: fechas en JSON con sufijo "Z" (UTC) pasan a la hora local del servidor.
/// </summary>
/// <remarks>
/// System.Text.Json convierte a hora local las fechas con offset ("2026-11-17T12:00:00-06:00", Kind = Local),
/// pero deja tal cual las que terminan en "Z" (Kind = Utc). La API trabaja en hora local
/// (DateTimeProvider.Now = DateTime.Now y TZ=America/Mexico_City), así que un punto GPS o una carga
/// mandados en UTC quedaban 6 horas corridos y se rechazaban como "fuera del periodo del viaje".
/// La app manda offset, pero un teléfono con zona UTC manda "Z". Las fechas sin zona no cambian.
/// Es el equivalente JSON de <see cref="FechaFormularioExtensions.AHoraLocal"/> (formularios).
/// También aplica a DateTime? (System.Text.Json reutiliza el convertidor para el tipo anulable).
/// </remarks>
public sealed class FechaHoraLocalJsonConverter : JsonConverter<DateTime>
{
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => reader.GetDateTime().AHoraLocal();

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        => writer.WriteStringValue(value);
}
