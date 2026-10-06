namespace APIGasoline.Application.Features.CargasCombustible.Common;

/// <summary>
/// No se pueden cargar más litros de los que caben en el tanque.
/// Si el carro no tiene capacidad registrada, no se puede validar y se acepta.
/// </summary>
public static class ReglaCapacidadTanque
{
    public static bool Excede(decimal litros, decimal? capacidadTanque)
        => capacidadTanque is not null && litros > capacidadTanque;
}
