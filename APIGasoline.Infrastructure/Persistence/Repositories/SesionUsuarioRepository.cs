using APIGasoline.Domain.Entities;
using APIGasoline.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace APIGasoline.Infrastructure.Persistence.Repositories;

public class SesionUsuarioRepository : ISesionUsuarioRepository
{
    private readonly AppDbContext _context;

    public SesionUsuarioRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<SesionUsuario?> GetByIdAsync(Guid sesionUsuarioId, CancellationToken cancellationToken = default)
        => _context.Sesiones.FirstOrDefaultAsync(s => s.SesionUsuarioId == sesionUsuarioId, cancellationToken);

    public Task<SesionUsuario?> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default)
        => _context.Sesiones.FirstOrDefaultAsync(
            s => s.TokenHash == tokenHash || s.TokenHashAnterior == tokenHash,
            cancellationToken);

    public async Task<IReadOnlyList<SesionUsuario>> GetActivasByUsuarioAsync(Guid usuarioId, DateTime ahora, CancellationToken cancellationToken = default)
        => await _context.Sesiones
            .Where(s => s.UsuarioId == usuarioId && s.FechaRevocacion == null && s.ExpiraEn > ahora)
            .OrderByDescending(s => s.FechaUltimoUso)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<SesionUsuario>> GetActivasByEmpresaAsync(Guid empresaId, DateTime ahora, CancellationToken cancellationToken = default)
        => await _context.Sesiones
            .Where(s => s.EmpresaId == empresaId && s.FechaRevocacion == null && s.ExpiraEn > ahora)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(SesionUsuario sesion, CancellationToken cancellationToken = default)
        => await _context.Sesiones.AddAsync(sesion, cancellationToken);

    public Task<int> EliminarObsoletasAsync(DateTime limite, CancellationToken cancellationToken = default)
        => _context.Sesiones
            .Where(s => s.ExpiraEn < limite || (s.FechaRevocacion != null && s.FechaRevocacion < limite))
            .ExecuteDeleteAsync(cancellationToken);
}
