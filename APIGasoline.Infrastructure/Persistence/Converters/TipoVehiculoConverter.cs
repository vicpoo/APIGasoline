using APIGasoline.Domain.Enums;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace APIGasoline.Infrastructure.Persistence.Converters;

/// <summary>
/// TipoVehiculo ⇄ 'carro' | 'moto' | 'camioneta' | 'camion'.
/// </summary>
public class TipoVehiculoConverter : ValueConverter<TipoVehiculo, string>
{
    /// <summary>Valores permitidos en la columna (varchar + CHECK, ver TiposSql.EsEnum).</summary>
    public static readonly string[] Valores = ["carro", "moto", "camioneta", "camion"];
    public const string Default = "carro";

    public TipoVehiculoConverter()
        : base(
            v => v == TipoVehiculo.Moto ? "moto"
               : v == TipoVehiculo.Camioneta ? "camioneta"
               : v == TipoVehiculo.Camion ? "camion"
               : "carro",
            v => v == "moto" ? TipoVehiculo.Moto
               : v == "camioneta" ? TipoVehiculo.Camioneta
               : v == "camion" ? TipoVehiculo.Camion
               : TipoVehiculo.Carro)
    {
    }
}
