using System.Text.RegularExpressions;
using APIGasoline.Domain.Enums;
using APIGasoline.Domain.Exceptions;

namespace APIGasoline.Domain.Entities;

/// <summary>
/// Acción que puede ejecutar un rol dentro del sistema.
/// La <see cref="Clave"/> (modulo.accion) es INMUTABLE: es lo que usan la API y el front para
/// decidir permisos. El <see cref="Nombre"/> es solo el texto visible y se puede cambiar.
/// </summary>
public partial class Privilegio
{
    public const int ClaveMaxLength = 100;
    public const int ModuloMaxLength = 50;
    public const int NombreMaxLength = 100;
    public const int DescripcionMaxLength = 255;

    public Guid PrivilegioId { get; private set; } = Guid.CreateVersion7();

    /// <summary>Código técnico inmutable con formato modulo.accion (ej. carro.crear, viaje.ver_todos).</summary>
    public string Clave { get; private set; } = string.Empty;

    /// <summary>Agrupa los privilegios en la pantalla de roles (carro, viaje, combustible...).</summary>
    public string Modulo { get; private set; } = string.Empty;

    public string Nombre { get; private set; } = string.Empty;
    public string? Descripcion { get; private set; }

    public AmbitoPrivilegio Ambito { get; private set; } = AmbitoPrivilegio.Empresa;

    public bool Activo { get; private set; } = true;

    // Requerido por Entity Framework
    private Privilegio()
    {
    }

    /// <param name="modulo">Si no se manda, se toma el primer segmento de la clave (carro.crear → carro).</param>
    public static Privilegio Crear(
        string clave,
        string nombre,
        string? descripcion,
        AmbitoPrivilegio ambito = AmbitoPrivilegio.Empresa,
        bool activo = true,
        string? modulo = null)
    {
        var privilegio = new Privilegio();
        privilegio.AsignarClave(clave);
        privilegio.CambiarModulo(modulo);
        privilegio.CambiarNombre(nombre);
        privilegio.CambiarDescripcion(descripcion);
        privilegio.CambiarAmbito(ambito);
        privilegio.Activo = activo;

        return privilegio;
    }

    /// <summary>Un privilegio de plataforma solo se puede asignar a roles globales.</summary>
    public bool AsignableA(Rol rol) => Ambito == AmbitoPrivilegio.Empresa || rol.EsGlobal;

    // La clave solo se asigna al crear: no existe un método para cambiarla.
    private void AsignarClave(string clave)
    {
        var normalizada = clave?.Trim().ToLowerInvariant() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(normalizada))
            throw new DomainException("La clave del privilegio es obligatoria.");

        if (normalizada.Length > ClaveMaxLength)
            throw new DomainException($"La clave del privilegio no puede exceder {ClaveMaxLength} caracteres.");

        if (!FormatoClave().IsMatch(normalizada))
            throw new DomainException(
                "La clave del privilegio debe tener el formato modulo.accion, solo con minúsculas, números y guion bajo (ej. carro.crear, viaje.ver_todos).");

        Clave = normalizada;
    }

    /// <summary>Si el módulo viene vacío se toma el primer segmento de la clave.</summary>
    public void CambiarModulo(string? modulo)
    {
        var normalizado = string.IsNullOrWhiteSpace(modulo)
            ? Clave.Split('.')[0]
            : modulo.Trim().ToLowerInvariant();

        if (string.IsNullOrWhiteSpace(normalizado))
            throw new DomainException("El módulo del privilegio es obligatorio.");

        if (normalizado.Length > ModuloMaxLength)
            throw new DomainException($"El módulo del privilegio no puede exceder {ModuloMaxLength} caracteres.");

        if (!FormatoModulo().IsMatch(normalizado))
            throw new DomainException("El módulo del privilegio solo puede tener minúsculas, números y guion bajo (ej. carro, viaje).");

        Modulo = normalizado;
    }

    public void CambiarNombre(string nombre)
    {
        var normalizado = nombre?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(normalizado))
            throw new DomainException("El nombre del privilegio es obligatorio.");

        if (normalizado.Length > NombreMaxLength)
            throw new DomainException($"El nombre del privilegio no puede exceder {NombreMaxLength} caracteres.");

        Nombre = normalizado;
    }

    public void CambiarDescripcion(string? descripcion)
    {
        var normalizada = string.IsNullOrWhiteSpace(descripcion) ? null : descripcion.Trim();

        if (normalizada is not null && normalizada.Length > DescripcionMaxLength)
            throw new DomainException($"La descripción del privilegio no puede exceder {DescripcionMaxLength} caracteres.");

        Descripcion = normalizada;
    }

    public void CambiarAmbito(AmbitoPrivilegio ambito)
    {
        if (!Enum.IsDefined(ambito))
            throw new DomainException("El ámbito del privilegio no es válido.");

        Ambito = ambito;
    }

    public void Activar() => Activo = true;

    public void Desactivar() => Activo = false;

    // modulo.accion[.subaccion]: cada segmento empieza con letra
    [GeneratedRegex(@"^[a-z][a-z0-9_]*(\.[a-z][a-z0-9_]*)+$")]
    private static partial Regex FormatoClave();

    [GeneratedRegex(@"^[a-z][a-z0-9_]*$")]
    private static partial Regex FormatoModulo();
}
