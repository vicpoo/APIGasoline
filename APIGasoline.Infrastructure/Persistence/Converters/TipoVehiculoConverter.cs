using APIGasoline.Domain.Enums;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace APIGasoline.Infrastructure.Persistence.Converters;

/// <summary>
/// TipoVehiculo ⇄ enum('carro','moto','camioneta','camion') de MySQL.
/// </summary>
public class TipoVehiculoConverter : ValueConverter<TipoVehiculo, string>
{
    public const string ColumnType = "enum('carro','moto','camioneta','camion')";
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
