using APIGasoline.Domain.Enums;
using APIGasoline.Domain.Exceptions;

namespace APIGasoline.Domain.Entities;

/// <summary>
/// Sesión de un usuario en un dispositivo (una fila por inicio de sesión).
///
/// - El refresh token NUNCA se guarda: solo su hash SHA-256 (<see cref="TokenHash"/>).
/// - Rotación: cada renovación reemplaza el token. El hash anterior se conserva en
///   <see cref="TokenHashAnterior"/> para detectar si alguien vuelve a usar un token ya reemplazado
///   (señal de robo): en ese caso se revoca la sesión completa.
/// - Expiración deslizante: cada renovación extiende <see cref="ExpiraEn"/>, pero nunca más allá
///   de <see cref="ExpiraAbsolutaEn"/> (tope desde el inicio de sesión).
/// </summary>
public class SesionUsuario
{
    /// <summary>SHA-256 en hexadecimal (64 caracteres).</summary>
    public const int TokenHashLength = 64;
    public const int NombreDispositivoMaxLength = 100;
    public const int IpMaxLength = 45; // IPv6
    public const int UserAgentMaxLength = 255;

    public Guid SesionUsuarioId { get; private set; } = Guid.CreateVersion7();
    public Guid UsuarioId { get; private set; }

    /// <summary>Empresa del empleado al iniciar sesión (null = usuario sin empleado). Sirve para revocar por empresa.</summary>
    public Guid? EmpresaId { get; private set; }

    public string TokenHash { get; private set; } = string.Empty;
    public string? TokenHashAnterior { get; private set; }

    public PlataformaDispositivo? Plataforma { get; private set; }
    public string? NombreDispositivo { get; private set; }
    public string? Ip { get; private set; }
    public string? UserAgent { get; private set; }

    public DateTime FechaCreacion { get; private set; }
    public DateTime FechaUltimoUso { get; private set; }
    public DateTime? FechaRotacion { get; private set; }
    public DateTime ExpiraEn { get; private set; }
    public DateTime ExpiraAbsolutaEn { get; private set; }

    public DateTime? FechaRevocacion { get; private set; }
    public MotivoRevocacionSesion? MotivoRevocacion { get; private set; }

    /// <summary>Quién la cerró (null = el sistema o el propio dispositivo al hacer logout).</summary>
    public Guid? RevocadaPorUsuarioId { get; private set; }

    // Requerido por Entity Framework
    private SesionUsuario()
    {
    }

    public bool EstaRevocada => FechaRevocacion is not null;

    public bool EstaActiva(DateTime ahora) => !EstaRevocada && ahora < ExpiraEn;

    public static SesionUsuario Iniciar(
        Guid usuarioId,
        Guid? empresaId,
        string tokenHash,
        PlataformaDispositivo? plataforma,
        string? nombreDispositivo,
        string? ip,
        string? userAgent,
        DateTime ahora,
        TimeSpan duracion,
        TimeSpan duracionMaxima)
    {
        if (usuarioId == Guid.Empty)
            throw new DomainException("El id del usuario es obligatorio.");

        if (empresaId == Guid.Empty)
            throw new DomainException("El id de la empresa no es válido.");

        if (ahora == default)
            throw new DomainException("La fecha de inicio de la sesión es obligatoria.");

        if (duracion <= TimeSpan.Zero)
            throw new DomainException("La duración de la sesión debe ser mayor a cero.");

        if (duracionMaxima < duracion)
            throw new DomainException("La duración máxima de la sesión no puede ser menor a la duración.");

        if (plataforma is not null && !Enum.IsDefined(plataforma.Value))
            throw new DomainException("La plataforma del dispositivo no es válida.");

        var sesion = new SesionUsuario
        {
            UsuarioId = usuarioId,
            EmpresaId = empresaId,
            TokenHash = ValidarHash(tokenHash),
            Plataforma = plataforma,
            FechaCreacion = ahora,
            FechaUltimoUso = ahora,
            ExpiraAbsolutaEn = ahora + duracionMaxima,
            ExpiraEn = ahora + duracion
        };

        sesion.ActualizarCliente(nombreDispositivo, ip, userAgent);

        return sesion;
    }

    /// <summary>true si el hash corresponde al token anterior (ya reemplazado) y no al actual.</summary>
    public bool EsTokenAnterior(string tokenHash)
        => TokenHashAnterior is not null
           && !string.Equals(TokenHash, tokenHash, StringComparison.Ordinal)
           && string.Equals(TokenHashAnterior, tokenHash, StringComparison.Ordinal);

    /// <summary>
    /// true si la última rotación fue hace menos de <paramref name="gracia"/>. Sirve para no confundir
    /// con un robo dos peticiones de renovación casi simultáneas del mismo celular.
    /// </summary>
    public bool RotadaRecientemente(DateTime ahora, TimeSpan gracia)
        => FechaRotacion is not null && ahora - FechaRotacion.Value <= gracia;

    /// <summary>
    /// Reemplaza el refresh token (rotación) y extiende la expiración deslizante sin pasar del tope absoluto.
    /// </summary>
    public void Rotar(string nuevoTokenHash, DateTime ahora, TimeSpan duracion, string? ip, string? userAgent)
    {
        if (!EstaActiva(ahora))
            throw new DomainException("La sesión ya expiró o fue cerrada.");

        if (duracion <= TimeSpan.Zero)
            throw new DomainException("La duración de la sesión debe ser mayor a cero.");

        var hash = ValidarHash(nuevoTokenHash);

        if (string.Equals(hash, TokenHash, StringComparison.Ordinal))
            throw new DomainException("El nuevo token debe ser diferente al actual.");

        TokenHashAnterior = TokenHash;
        TokenHash = hash;
        FechaRotacion = ahora;
        FechaUltimoUso = ahora;

        var nuevaExpiracion = ahora + duracion;
        ExpiraEn = nuevaExpiracion < ExpiraAbsolutaEn ? nuevaExpiracion : ExpiraAbsolutaEn;

        ActualizarCliente(NombreDispositivo, ip ?? Ip, userAgent ?? UserAgent);
    }

    /// <summary>Cierra la sesión. Si ya estaba cerrada no cambia nada (se conserva el primer motivo).</summary>
    public void Revocar(MotivoRevocacionSesion motivo, DateTime ahora, Guid? revocadaPorUsuarioId = null)
    {
        if (!Enum.IsDefined(motivo))
            throw new DomainException("El motivo de cierre de la sesión no es válido.");

        if (ahora == default)
            throw new DomainException("La fecha de cierre de la sesión es obligatoria.");

        if (EstaRevocada)
            return;

        FechaRevocacion = ahora;
        MotivoRevocacion = motivo;
        RevocadaPorUsuarioId = revocadaPorUsuarioId;
    }

    private void ActualizarCliente(string? nombreDispositivo, string? ip, string? userAgent)
    {
        NombreDispositivo = Recortar(nombreDispositivo, NombreDispositivoMaxLength);
        Ip = Recortar(ip, IpMaxLength);
        UserAgent = Recortar(userAgent, UserAgentMaxLength);
    }

    private static string ValidarHash(string tokenHash)
    {
        var hash = tokenHash?.Trim() ?? string.Empty;

        if (hash.Length != TokenHashLength)
            throw new DomainException($"El hash del token debe tener {TokenHashLength} caracteres.");

        return hash;
    }

    /// <summary>Datos informativos del cliente: se recortan en vez de rechazar el inicio de sesión.</summary>
    private static string? Recortar(string? valor, int maximo)
    {
        var texto = valor?.Trim();

        if (string.IsNullOrEmpty(texto))
            return null;

        return texto.Length > maximo ? texto[..maximo] : texto;
    }
}
