using Microsoft.Extensions.Configuration.Json;

namespace APIGasoline.Configuration;

/// <summary>
/// Agrega appsettings.Local.json (opcional, en .gitignore) para los valores personales de cada
/// desarrollador. Queda justo después de appsettings.{Ambiente}.json: le gana a los appsettings
/// del repo, pero las variables de entorno (Aspire, Docker) y la línea de comandos le siguen ganando.
/// </summary>
public static class ConfiguracionLocal
{
    public const string Archivo = "appsettings.Local.json";

    public static void Agregar(ConfigurationManager configuracion, IHostEnvironment ambiente)
    {
        var fuentes = (IList<IConfigurationSource>)configuracion.Sources;

        var archivoAmbiente = $"appsettings.{ambiente.EnvironmentName}.json";
        var posicion = -1;

        for (var i = 0; i < fuentes.Count; i++)
        {
            if (fuentes[i] is JsonConfigurationSource json
                && string.Equals(json.Path, archivoAmbiente, StringComparison.OrdinalIgnoreCase))
            {
                posicion = i;
            }
        }

        var local = new JsonConfigurationSource
        {
            Path = Archivo,
            Optional = true,
            ReloadOnChange = true,
            FileProvider = ambiente.ContentRootFileProvider,
        };

        if (posicion < 0)
            configuracion.Sources.Add(local);
        else
            fuentes.Insert(posicion + 1, local);
    }
}
