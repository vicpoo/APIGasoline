using APIGasoline.Domain.Enums;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace APIGasoline.Infrastructure.Persistence.Converters;

/// <summary>
/// TipoMotivoCambio ⇄ 'liberacion' | 'falla_mecanica' | 'choque_o_golpe' | 'servicio' | 'emergencia' | 'otro'.
/// </summary>
public class TipoMotivoCambioConverter : ValueConverter<TipoMotivoCambio, string>
{
    /// <summary>Valores permitidos en la columna (varchar + CHECK, ver TiposSql.EsEnum).</summary>
    public static readonly string[] Valores = ["liberacion", "falla_mecanica", "choque_o_golpe", "servicio", "emergencia", "otro"];

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
