using APIGasoline.Domain.Enums;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace APIGasoline.Infrastructure.Persistence.Converters;

/// <summary>
/// TipoMotivoCambio ⇄ enum('liberacion','falla_mecanica','choque_o_golpe','servicio','emergencia','otro') de MySQL.
/// </summary>
public class TipoMotivoCambioConverter : ValueConverter<TipoMotivoCambio, string>
{
    public const string ColumnType = "enum('liberacion','falla_mecanica','choque_o_golpe','servicio','emergencia','otro')";

    public TipoMotivoCambioConverter()
        : base(
            v => v == TipoMotivoCambio.Liberacion ? "liberacion"
               : v == TipoMotivoCambio.FallaMecanica ? "falla_mecanica"
               : v == TipoMotivoCambio.ChoqueOGolpe ? "choque_o_golpe"
               : v == TipoMotivoCambio.Servicio ? "servicio"
               : v == TipoMotivoCambio.Emergencia ? "emergencia"
               : "otro",
            v => v == "liberacion" ? TipoMotivoCambio.Liberacion
               : v == "falla_mecanica" ? TipoMotivoCambio.FallaMecanica
               : v == "choque_o_golpe" ? TipoMotivoCambio.ChoqueOGolpe
               : v == "servicio" ? TipoMotivoCambio.Servicio
               : v == "emergencia" ? TipoMotivoCambio.Emergencia
               : TipoMotivoCambio.Otro)
    {
    }
}
