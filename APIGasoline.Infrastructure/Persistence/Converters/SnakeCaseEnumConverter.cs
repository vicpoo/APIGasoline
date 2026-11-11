using System.Text;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace APIGasoline.Infrastructure.Persistence.Converters;

/// <summary>
/// Guarda un enum de C# como texto en snake_case (CambiarEstado ⇄ 'cambiar_estado'),
/// para columnas de texto con valores fijos (varchar + CHECK, ver TiposSql.EsEnum).
/// </summary>
public class SnakeCaseEnumConverter<TEnum> : ValueConverter<TEnum, string>
    where TEnum : struct, Enum
{
    public SnakeCaseEnumConverter()
        : base(v => ToSnakeCase(v), v => FromSnakeCase(v))
    {
    }

    /// <summary>Todos los valores del enum en snake_case (para TiposSql.EsEnum: varchar + CHECK).</summary>
    public static string[] Valores
        => Enum.GetValues<TEnum>().Select(ToSnakeCase).ToArray();

    public static string ToSnakeCase(TEnum valor)
    {
        var nombre = valor.ToString();
        var sb = new StringBuilder(nombre.Length + 8);

        for (var i = 0; i < nombre.Length; i++)
        {
            var c = nombre[i];

            if (char.IsUpper(c) && i > 0)
                sb.Append('_');

            sb.Append(char.ToLowerInvariant(c));
        }

        return sb.ToString();
    }

    public static TEnum FromSnakeCase(string valor)
        => Enum.Parse<TEnum>(valor.Replace("_", string.Empty), ignoreCase: true);
}
