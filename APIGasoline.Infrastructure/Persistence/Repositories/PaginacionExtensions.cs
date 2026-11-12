using APIGasoline.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace APIGasoline.Infrastructure.Persistence.Repositories;

public static class PaginacionExtensions
{
    /// <summary>
    /// Cuenta el total y trae solo la página pedida (OFFSET/FETCH en SQL Server).
    /// La consulta debe venir ordenada de forma determinista (con el id como desempate).
    /// </summary>
    public static async Task<PaginaDe<T>> APaginaAsync<T>(this IQueryable<T> consulta, Paginacion pagina, CancellationToken cancellationToken = default)
    {
        var total = await consulta.CountAsync(cancellationToken);
        var elementos = total == 0
            ? []
            : await consulta.Skip(pagina.Saltar).Take(pagina.TamanoPagina).ToListAsync(cancellationToken);

        return new PaginaDe<T>(elementos, total, pagina);
    }
}
