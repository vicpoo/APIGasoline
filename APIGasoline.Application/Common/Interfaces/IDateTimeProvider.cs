namespace APIGasoline.Application.Common.Interfaces;

/// <summary>
/// Abstrae la hora actual para no depender de DateTime.Now en la lógica (y poder probarla).
/// </summary>
public interface IDateTimeProvider
{
    DateTime Now { get; }
}
