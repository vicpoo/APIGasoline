using APIGasoline.Domain.Exceptions;

namespace APIGasoline.Domain.Entities;

/// <summary>
/// Representa al corporativo al que pueden pertenecer una o varias empresas.
/// </summary>
public class Corporativo
{
    public const int NombreMaxLength = 150;

    public int CorporativoId { get; private set; }
    public string Nombre { get; private set; } = string.Empty;
    public bool Activo { get; private set; } = true;

    /// <summary>
    /// La asigna la base de datos al insertar (default now()). No se modifica después.
    /// </summary>
    public DateTime? FechaRegistro { get; private set; }

    // Requerido por Entity Framework
    private Corporativo()
    {
    }

    public static Corporativo Crear(string nombre, bool activo = true)
    {
        var corporativo = new Corporativo();
        corporativo.CambiarNombre(nombre);
        corporativo.Activo = activo;

        return corporativo;
    }

    public void CambiarNombre(string nombre)
    {
        var normalizado = nombre?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(normalizado))
            throw new DomainException("El nombre del corporativo es obligatorio.");

        if (normalizado.Length > NombreMaxLength)
            throw new DomainException($"El nombre del corporativo no puede exceder {NombreMaxLength} caracteres.");

        Nombre = normalizado;
    }

    public void Activar() => Activo = true;

    public void Desactivar() => Activo = false;
}
