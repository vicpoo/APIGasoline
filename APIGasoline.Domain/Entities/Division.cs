using APIGasoline.Domain.Exceptions;

namespace APIGasoline.Domain.Entities;

/// <summary>
/// Una división pertenece a un corporativo.
/// </summary>
public class Division
{
    public const int NombreMaxLength = 100;

    public int DivisionId { get; private set; }
    public int CorporativoId { get; private set; }
    public string Nombre { get; private set; } = string.Empty;
    public bool Activo { get; private set; } = true;

    /// <summary>
    /// La asigna la base de datos al insertar (default now()). No se modifica después.
    /// </summary>
    public DateTime? FechaRegistro { get; private set; }

    // Requerido por Entity Framework
    private Division()
    {
    }

    public static Division Crear(int corporativoId, string nombre, bool activo = true)
    {
        var division = new Division();
        division.CambiarCorporativo(corporativoId);
        division.CambiarNombre(nombre);
        division.Activo = activo;

        return division;
    }

    public void CambiarCorporativo(int corporativoId)
    {
        if (corporativoId <= 0)
            throw new DomainException("El id del corporativo debe ser mayor a cero.");

        CorporativoId = corporativoId;
    }

    public void CambiarNombre(string nombre)
    {
        var normalizado = nombre?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(normalizado))
            throw new DomainException("El nombre de la división es obligatorio.");

        if (normalizado.Length > NombreMaxLength)
            throw new DomainException($"El nombre de la división no puede exceder {NombreMaxLength} caracteres.");

        Nombre = normalizado;
    }

    public void Activar() => Activo = true;

    public void Desactivar() => Activo = false;
}
