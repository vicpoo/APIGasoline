using APIGasoline.Domain.Entities;
using APIGasoline.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace APIGasoline.Infrastructure.Persistence.Repositories;

public class EmpresaRepository : IEmpresaRepository
{
    private readonly AppDbContext _context;

    public EmpresaRepository(AppDbContext context)
    {
        _context = context;
    }

    // El filtro global (HasQueryFilter) ya excluye las empresas eliminadas.

    public Task<Empresa?> GetByIdAsync(Guid empresaId, CancellationToken cancellationToken = default)
        => _context.Empresas.FirstOrDefaultAsync(e => e.EmpresaId == empresaId, cancellationToken);

    public async Task<IReadOnlyList<Empresa>> GetAllAsync(Guid? corporativoId = null, Guid? divisionId = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Empresas.AsNoTracking();

        if (corporativoId is not null)
            query = query.Where(e => e.CorporativoId == corporativoId);

        if (divisionId is not null)
            query = query.Where(e => e.DivisionId == divisionId);

        return await query
            .OrderBy(e => e.EmpresaId)
            .ToListAsync(cancellationToken);
    }

    public Task<bool> ExistsAsync(Guid empresaId, CancellationToken cancellationToken = default)
        => _context.Empresas.AnyAsync(e => e.EmpresaId == empresaId, cancellationToken);

    public Task<bool> ExistsByCorporativoAsync(Guid corporativoId, CancellationToken cancellationToken = default)
        => _context.Empresas
            .IgnoreQueryFilters()
            .AnyAsync(e => e.CorporativoId == corporativoId, cancellationToken);

    public Task<bool> ExistsByDivisionAsync(Guid divisionId, CancellationToken cancellationToken = default)
        => _context.Empresas
            .IgnoreQueryFilters()
            .AnyAsync(e => e.DivisionId == divisionId, cancellationToken);

    public async Task AddAsync(Empresa empresa, CancellationToken cancellationToken = default)
        => await _context.Empresas.AddAsync(empresa, cancellationToken);
}
