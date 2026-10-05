using APIGasoline.Domain.Entities;
using APIGasoline.Domain.Enums;

namespace APIGasoline.Domain.Interfaces;

public interface ICarroRepository
{
    Task<Carro?> GetByIdAsync(int carroId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Carro>> GetAllAsync(int? empresaId = null, EstadoCarro? estado = null, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(int carroId, CancellationToken cancellationToken = default);
    Task<bool> PlacasExistsAsync(string placas, int? excludeCarroId = null, CancellationToken cancellationToken = default);
    Task AddAsync(Carro carro, CancellationToken cancellationToken = default);
    void Remove(Carro carro);
}
