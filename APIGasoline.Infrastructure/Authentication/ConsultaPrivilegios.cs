using APIGasoline.Application.Common.Interfaces;
using APIGasoline.Domain.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace APIGasoline.Infrastructure.Authentication;

/// <summary>
/// Privilegios efectivos del usuario. La consulta va por el Unit of Work (repositorio de privilegios)
/// y el resultado se guarda en memoria unos segundos (<see cref="DuracionCache"/>) para no ir a la BD
/// en cada petición. Si un administrador cambia los privilegios de un rol, aplican en ese tiempo
/// sin volver a iniciar sesión.
/// </summary>
public sealed class ConsultaPrivilegios : IConsultaPrivilegios
{
    public static readonly TimeSpan DuracionCache = TimeSpan.FromSeconds(30);

    private readonly IUnitOfWork _unitOfWork;
    private readonly IMemoryCache _cache;

    public ConsultaPrivilegios(IUnitOfWork unitOfWork, IMemoryCache cache)
    {
        _unitOfWork = unitOfWork;
        _cache = cache;
    }

    public async Task<IReadOnlySet<string>> ObtenerAsync(Guid usuarioId, Guid? empresaId, CancellationToken cancellationToken = default)
    {
        var llave = $"privilegios:{usuarioId}:{empresaId}";

        if (_cache.TryGetValue(llave, out IReadOnlySet<string>? enCache) && enCache is not null)
            return enCache;

        var claves = await _unitOfWork.Privilegios.GetClavesEfectivasAsync(usuarioId, empresaId, cancellationToken);
        IReadOnlySet<string> privilegios = new HashSet<string>(claves, StringComparer.Ordinal);

        _cache.Set(llave, privilegios, DuracionCache);

        return privilegios;
    }
}
