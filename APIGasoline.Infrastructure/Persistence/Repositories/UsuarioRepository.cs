using System;
using System.Collections.Generic;
using System.Text;

using APIGasoline.Domain.Entities;
using APIGasoline.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace APIGasoline.Infrastructure.Persistence.Repositories;

public class UsuarioRepository : IUsuarioRepository
{
    private readonly AppDbContext _context;

    public UsuarioRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<Usuario?> GetByIdAsync(int usuarioId, CancellationToken cancellationToken = default)
        => _context.Usuarios.FirstOrDefaultAsync(u => u.UsuarioId == usuarioId, cancellationToken);

    public async Task<IReadOnlyList<Usuario>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _context.Usuarios
            .AsNoTracking()
            .OrderBy(u => u.UsuarioId)
            .ToListAsync(cancellationToken);

    public Task<bool> ExistsAsync(int usuarioId, CancellationToken cancellationToken = default)
        => _context.Usuarios.AnyAsync(u => u.UsuarioId == usuarioId, cancellationToken);

    public Task<bool> EmailExistsAsync(string email, int? excludeUsuarioId = null, CancellationToken cancellationToken = default)
        => _context.Usuarios.AnyAsync(
            u => u.Email == email && (excludeUsuarioId == null || u.UsuarioId != excludeUsuarioId),
            cancellationToken);

    public async Task AddAsync(Usuario usuario, CancellationToken cancellationToken = default)
        => await _context.Usuarios.AddAsync(usuario, cancellationToken);

    public void Remove(Usuario usuario)
        => _context.Usuarios.Remove(usuario);
}