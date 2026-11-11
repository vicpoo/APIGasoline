using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace APIGasoline.Infrastructure.Persistence;

/// <summary>
/// Tipos de columna de SQL Server que se repiten en las configuraciones de EF.
/// </summary>
public static class TiposSql
{
    /// <summary>
    /// Fecha y hora con precisión de segundos (igual que el timestamp de MySQL que se usaba antes).
    /// OJO: en SQL Server "timestamp" es rowversion, no una fecha.
    /// </summary>
    public const string FechaHora = "datetime2(0)";

    /// <summary>Default "ahora" con la hora local del servidor de BD (variable TZ del contenedor).</summary>
    public const string Ahora = "SYSDATETIME()";

    /// <summary>Texto largo (antes text de MySQL).</summary>
    public const string TextoLargo = "nvarchar(max)";

    /// <summary>JSON guardado como texto; <see cref="EsJson{T}"/> agrega el CHECK ISJSON.</summary>
    public const string Json = "nvarchar(max)";

    internal const string AnotacionValoresEnum = "APIGasoline:ValoresEnum";
    internal const string AnotacionJson = "APIGasoline:Json";

    /// <summary>
    /// Columna de texto con un conjunto fijo de valores (antes enum('a','b') de MySQL):
    /// varchar del largo del valor más grande + CHECK (col IN ('a','b')) que crea
    /// <see cref="AplicarRestricciones"/>. Así la BD sigue rechazando valores inválidos.
    /// </summary>
    public static PropertyBuilder<T> EsEnum<T>(this PropertyBuilder<T> propiedad, params string[] valores)
    {
        if (valores.Length == 0)
            throw new ArgumentException("Un enum necesita al menos un valor.", nameof(valores));

        return propiedad
            .HasColumnType($"varchar({valores.Max(v => v.Length)})")
            .HasAnnotation(AnotacionValoresEnum, string.Join(",", valores));
    }

    /// <summary>JSON: nvarchar(max) + CHECK (col IS NULL OR ISJSON(col) = 1), como validaba el tipo json de MySQL.</summary>
    public static PropertyBuilder<T> EsJson<T>(this PropertyBuilder<T> propiedad)
        => propiedad
            .HasColumnType(Json)
            .HasAnnotation(AnotacionJson, true);

    /// <summary>
    /// Crea los CHECK de las columnas marcadas con <see cref="EsEnum{T}"/> y <see cref="EsJson{T}"/>.
    /// Se llama en OnModelCreating después de aplicar todas las configuraciones (ya se conocen
    /// los nombres de tabla y columna). Las anotaciones se quitan para que no lleguen a las migraciones.
    /// </summary>
    public static void AplicarRestricciones(ModelBuilder modelBuilder)
    {
        foreach (var entidad in modelBuilder.Model.GetEntityTypes())
        {
            var tabla = entidad.GetTableName();
            if (tabla is null)
                continue;

            var almacen = StoreObjectIdentifier.Table(tabla, entidad.GetSchema());

            foreach (var propiedad in entidad.GetDeclaredProperties())
            {
                var columna = propiedad.GetColumnName(almacen);

                if (propiedad.FindAnnotation(AnotacionValoresEnum)?.Value is string lista)
                {
                    var valores = string.Join(", ", lista.Split(',').Select(v => $"'{v}'"));
                    entidad.AddCheckConstraint($"ck_{tabla}_{columna}", $"[{columna}] IN ({valores})");
                    propiedad.RemoveAnnotation(AnotacionValoresEnum);
                }

                if (propiedad.FindAnnotation(AnotacionJson) is not null)
                {
                    entidad.AddCheckConstraint($"ck_{tabla}_{columna}_json", $"[{columna}] IS NULL OR ISJSON([{columna}]) = 1");
                    propiedad.RemoveAnnotation(AnotacionJson);
                }
            }
        }
    }
}
