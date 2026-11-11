namespace APIGasoline.Application.Common.Interfaces;

/// <summary>
/// De qué empresa son los datos que puede ver y tocar la petición actual. Lo usan el filtro global
/// de EF Core (cada consulta trae solo lo de esa empresa) y el guardián de SaveChanges (no deja
/// crear, editar ni borrar nada de otra empresa).
///
/// - Usuario de una empresa: solo su empresa (la del token).
/// - Usuario con el privilegio plataforma.multiempresa: todas.
/// - Sin usuario (login, refresh, workers en segundo plano, siembra al arrancar): todas.
///
/// Si hay un usuario autenticado y el contexto no se inicializó (<see cref="InicializarAsync"/>),
/// leerlo lanza una excepción: ante la duda NO se muestra nada (falla cerrado).
/// </summary>
public interface IContextoEmpresa
{
    /// <summary>true: la petición no se limita a una empresa (plataforma o sistema).</summary>
    bool VeTodasLasEmpresas { get; }

    /// <summary>
    /// Empresa a la que se limita la petición cuando <see cref="VeTodasLasEmpresas"/> es false.
    /// Guid.Empty si el usuario no tiene empresa (entonces no ve nada).
    /// </summary>
    Guid EmpresaId { get; }

    /// <summary>Lee los privilegios del usuario una vez por petición. La API lo llama justo después de autenticar.</summary>
    Task InicializarAsync(CancellationToken cancellationToken = default);
}
