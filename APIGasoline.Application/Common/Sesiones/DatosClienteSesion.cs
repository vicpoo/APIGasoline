using APIGasoline.Domain.Enums;

namespace APIGasoline.Application.Common.Sesiones;

/// <summary>
/// Datos informativos del dispositivo que inicia o renueva la sesión. Solo sirven para que el
/// usuario reconozca sus sesiones abiertas ("Android · Moto G · 189.x.x.x"); nunca para autorizar.
/// </summary>
public record DatosClienteSesion(
    PlataformaDispositivo? Plataforma = null,
    string? NombreDispositivo = null,
    string? Ip = null,
    string? UserAgent = null)
{
    public static DatosClienteSesion Vacio { get; } = new();
}
