using APIGasoline.Domain.Exceptions;

namespace APIGasoline.Domain.Entities;

/// <summary>
/// Rol de autorización. Puede ser global (EmpresaId = null, disponible para todas las empresas)
/// o exclusivo de una empresa.
/// </summary>
public class Rol
{
    public const int NombreMaxLength = 100;
    public const int DescripcionMaxLength = 255;

    public int RolId { get; private set; }

    /// <summary>null = rol global. Con valor = rol exclusivo de esa empresa. No cambia después de crearse.</summary>
    public int? EmpresaId { get; private set; }

    public string Nombre { get; private set; } = string.Empty;
    public string? Descripcion { get; private set; }
    public bool Activo { get; private set; } = true;

    // Requerido por Entity Framework
    private Rol()
    {
    }

    public static Rol Crear(string nombre, string? descripcion, bool activo = true, int? empresaId = null)
    {
        if (empresaId is <= 0)
            throw new DomainException("El id de la empresa debe ser mayor a cero.");

        var rol = new Rol { EmpresaId = empresaId };
        rol.CambiarNombre(nombre);
        rol.CambiarDescripcion(descripcion);
        rol.Activo = activo;

        return rol;
    }

    public bool EsGlobal => EmpresaId is null;

    /// <summary>
    /// Un rol global se puede asignar a cualquiera; uno exclusivo solo a usuarios de su empresa.
    /// </summary>
    public bool DisponiblePara(int? empresaId) => EsGlobal || EmpresaId == empresaId;

    public void CambiarNombre(string nombre)
    {
        var normalizado = nombre?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(normalizado))
            throw new DomainException("El nombre del rol es obligatorio.");

        if (normalizado.Length > NombreMaxLength)
            throw new DomainException($"El nombre del rol no puede exceder {NombreMaxLength} caracteres.");

        Nombre = normalizado;
    }

    public void CambiarDescripcion(string? descripcion)
    {
        var normalizada = string.IsNullOrWhiteSpace(descripcion) ? null : descripcion.Trim();

        if (normalizada is not null && normalizada.Length > DescripcionMaxLength)
            throw new DomainException($"La descripción del rol no puede exceder {DescripcionMaxLength} caracteres.");

        Descripcion = normalizada;
    }

    public void Activar() => Activo = true;

    public void Desactivar() => Activo = false;
}
