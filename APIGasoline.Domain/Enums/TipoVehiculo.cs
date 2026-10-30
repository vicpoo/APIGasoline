namespace APIGasoline.Domain.Enums;

/// <summary>
/// Tipo de vehículo de la flota. La app cambia el ícono de la tarjeta según el tipo.
/// En MySQL: enum('carro','moto','camioneta','camion'). Los valores numéricos no se deben
/// reordenar (agrega los nuevos al final).
/// </summary>
public enum TipoVehiculo
{
    Carro = 0,
    Moto = 1,
    Camioneta = 2,
    Camion = 3
}
