using APIGasoline.Domain.Entities;
using APIGasoline.Domain.Enums;
using APIGasoline.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace APIGasoline.Infrastructure.Persistence.Repositories;

public class CarroRepository : ICarroRepository
{
    private readonly AppDbContext _context;

    public CarroRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<Carro?> GetByIdAsync(Guid carroId, CancellationToken cancellationToken = default)
        => _context.Carros.FirstOrDefaultAsync(c => c.CarroId == carroId, cancellationToken);

    public async Task<IReadOnlyList<Carro>> GetAllAsync(Guid? empresaId = null, EstadoCarro? estado = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Carros.AsNoTracking();

        if (empresaId is not null)
            query = query.Where(c => c.EmpresaId == empresaId);

        if (estado is not null)
            query = query.Where(c => c.Estado == estado);

        return await query
            .OrderBy(c => c.CarroId)
            .ToListAsync(cancellationToken);
    }

    public Task<bool> ExistsAsync(Guid carroId, CancellationToken cancellationToken = default)
        => _context.Carros.AnyAsync(c => c.CarroId == carroId, cancellationToken);

    public Task<bool> PlacasExistsAsync(string placas, Guid? excludeCarroId = null, CancellationToken cancellationToken = default)
        => _context.Carros.AnyAsync(
            c => c.Placas == placas && (excludeCarroId == null || c.CarroId != excludeCarroId),
            cancellationToken);

    public async Task AddAsync(Carro carro, CancellationToken cancellationToken = default)
        => await _context.Carros.AddAsync(carro, cancellationToken);

    public void Remove(Carro carro)
        => _context.Carros.Remove(carro);
}
