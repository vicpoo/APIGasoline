using System.Globalization;
using APIGasoline.Application.Features.Reportes.DTOs;
using APIGasoline.Domain.Enums;

namespace APIGasoline.Infrastructure.Reportes;

/// <summary>Textos y formatos que comparten el PDF y el Excel (mismo idioma y unidades que la app).</summary>
internal static class TextosReporte
{
    public static readonly CultureInfo Cultura = CultureInfo.GetCultureInfo("es-MX");

    public static string Titulo(ReporteFlotaDto r)
        => r.CarroId is not null && r.Carros.Count == 1
            ? $"Reporte del vehículo {r.Carros[0].Marca} {r.Carros[0].Modelo} ({r.Carros[0].Placas})"
            : "Reporte de la flota";

    public static string Periodo(ReporteFlotaDto r) => (r.Desde, r.Hasta) switch
    {
        (null, null) => "Todo el historial",
        ({ } desde, null) => $"Desde el {Fecha(desde)}",
        (null, { } hasta) => $"Hasta el {Fecha(hasta)}",
        ({ } desde, { } hasta) => $"Del {Fecha(desde)} al {Fecha(hasta)}",
    };

    public static string Fecha(DateTime fecha) => fecha.ToString("dd/MM/yyyy", Cultura);

    public static string FechaHora(DateTime fecha) => fecha.ToString("dd/MM/yyyy HH:mm", Cultura);

    public static string Numero(decimal valor) => valor.ToString("N2", Cultura);

    public static string Entero(int valor) => valor.ToString("N0", Cultura);

    public static string Dinero(decimal valor) => "$" + valor.ToString("N2", Cultura);

    public static string Opcional(decimal? valor, Func<decimal, string> formato) => valor is { } v ? formato(v) : "—";

    public static string Estado(CarroReporteDto carro) => carro.DadoDeBaja
        ? "Dado de baja"
        : carro.Estado switch
        {
            EstadoCarro.Disponible => "Disponible",
            EstadoCarro.Ocupado => "Ocupado",
            EstadoCarro.EnMantenimiento => "En mantenimiento",
            _ => carro.Estado.ToString(),
        };

    /// <summary>Texto de la columna "Centro de costos" cuando la carga (ni su viaje) tiene uno.</summary>
    public const string SinCentroCosto = "Sin centro de costos";

    public static string CentroCosto(CargaReporteDto carga) => carga.CentroCosto ?? SinCentroCosto;

    /// <summary>Precio por litro de una carga (— si no tiene monto).</summary>
    public static decimal? PrecioLitro(CargaReporteDto carga)
        => carga.Monto is { } monto && monto > 0 && carga.Litros > 0 ? Math.Round(monto / carga.Litros, 2) : null;

    public static string Vehiculo(CarroReporteDto carro)
        => carro.Anio is { } anio ? $"{carro.Marca} {carro.Modelo} {anio}" : $"{carro.Marca} {carro.Modelo}";
}
