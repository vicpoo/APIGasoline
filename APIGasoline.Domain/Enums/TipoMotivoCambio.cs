namespace APIGasoline.Domain.Enums;

/// <summary>
/// Clasifica un cambio de estatus del carro. Con <see cref="FallaMecanica"/> y
/// <see cref="ChoqueOGolpe"/> se obtiene el registro de percances sin una tabla aparte.
/// </summary>
public enum TipoMotivoCambio
{
    Liberacion = 0,
    FallaMecanica = 1,
    ChoqueOGolpe = 2,
    Servicio = 3,
    Emergencia = 4,
    Otro = 5
}
