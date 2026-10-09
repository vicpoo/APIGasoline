using APIGasoline.Domain.Entities;
using APIGasoline.Domain.Enums;

namespace APIGasoline.Domain.Interfaces;

public interface ICarroRepository
{
    Task<Carro?> GetByIdAsync(Guid carroId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Carro>> GetAllAsync(Guid? empresaId = null, EstadoCarro? estado = null, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid carroId, CancellationToken cancellationToken = default);
    Task<bool> PlacasExistsAsync(string placas, Guid? excludeCarroId = null, CancellationToken cancellationToken = default);
    Task AddAsync(Carro carro, CancellationToken cancellationToken = default);
    void Remove(Carro carro);
}
