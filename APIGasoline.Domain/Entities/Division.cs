using APIGasoline.Domain.Exceptions;

namespace APIGasoline.Domain.Entities;

/// <summary>
/// División de una empresa. Se relaciona con uno o varios corporativos de la misma empresa
/// (N:M, por <see cref="CorporativoDivision"/>).
/// </summary>
public class Division
{
    public const int NombreMaxLength = 100;

    public Guid DivisionId { get; private set; } = Guid.CreateVersion7();

    /// <summary>Empresa dueña de la división. No cambia.</summary>
    public Guid EmpresaId { get; private set; }

    public string Nombre { get; private set; } = string.Empty;
    public bool Activo { get; private set; } = true;

 
    public DateTime? FechaRegistro { get; private set; }

    // Requerido por Entity Framework
    private Division()
    {
    }

    public static Division Crear(Guid empresaId, string nombre, bool activo = true)
    {
        if (empresaId == Guid.Empty)
            throw new DomainException("El id de la empresa es obligatorio.");

        var division = new Division { EmpresaId = empresaId };
        division.CambiarNombre(nombre);
        division.Activo = activo;

        return division;
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
