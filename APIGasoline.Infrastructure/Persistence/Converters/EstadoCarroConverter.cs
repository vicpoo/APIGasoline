using APIGasoline.Domain.Enums;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace APIGasoline.Infrastructure.Persistence.Converters;

/// <summary>
/// EstadoCarro ⇄ 'disponible' | 'ocupado' | 'en_mantenimiento'.
/// Se reutiliza en carro y en cambio_estatus_carro.
/// </summary>
public class EstadoCarroConverter : ValueConverter<EstadoCarro, string>
{
    /// <summary>Valores permitidos en la columna (varchar + CHECK, ver TiposSql.EsEnum).</summary>
    public static readonly string[] Valores = ["disponible", "ocupado", "en_mantenimiento"];

    public EstadoCarroConverter()
        : base(
            v => v == EstadoCarro.Disponible ? "disponible"
               : v == EstadoCarro.Ocupado ? "ocupado"
               : "en_mantenimiento",
            v => v == "disponible" ? EstadoCarro.Disponible
               : v == "ocupado" ? EstadoCarro.Ocupado
               : EstadoCarro.EnMantenimiento)
    {
    }
}
