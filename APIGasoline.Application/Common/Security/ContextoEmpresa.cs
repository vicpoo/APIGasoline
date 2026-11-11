using APIGasoline.Application.Common.Interfaces;

namespace APIGasoline.Application.Common.Security;

/// <summary>
/// Implementación de <see cref="IContextoEmpresa"/> a partir del usuario del token.
/// Vive lo que dura la petición (scoped).
/// </summary>
public sealed class ContextoEmpresa : IContextoEmpresa
{
    private enum Estado
    {
        Pendiente,
        Inicializando,
        Listo,
    }

    private readonly IUsuarioActual _usuario;

    private Estado _estado = Estado.Pendiente;
    private bool _veTodas;
    private Guid _empresaId;

    public ContextoEmpresa(IUsuarioActual usuario)
    {
        _usuario = usuario;
    }

    public bool VeTodasLasEmpresas
    {
        get
        {
            return _estado switch
            {
                Estado.Listo => _veTodas,

                // Mientras se leen los privilegios del propio usuario (roles globales y de su empresa)
                Estado.Inicializando => true,

                // Sin usuario: login, refresh, workers, siembra. No se guarda el resultado por si
                // la petición se autentica después (así nunca queda "ve todo" por accidente).
                _ when !_usuario.EstaAutenticado => true,

                _ => throw SinInicializar(),
            };
        }
    }

    public Guid EmpresaId
    {
        get
        {
            if (VeTodasLasEmpresas)
                return Guid.Empty;

            return _empresaId;
        }
    }

    public async Task InicializarAsync(CancellationToken cancellationToken = default)
    {
        if (_estado == Estado.Listo || !_usuario.EstaAutenticado)
            return;

        _estado = Estado.Inicializando;

        try
        {
            _veTodas = await _usuario.TienePrivilegioAsync(Permisos.PlataformaMultiempresa, cancellationToken);
            _empresaId = _usuario.EmpresaId ?? Guid.Empty;
            _estado = Estado.Listo;
        }
        catch
        {
            _estado = Estado.Pendiente;
            throw;
        }
    }

    private static InvalidOperationException SinInicializar()
        => new("El contexto de empresa no se inicializó para este usuario. " +
               "La API debe llamar a IContextoEmpresa.InicializarAsync después de autenticar (ContextoEmpresaMiddleware).");
}
