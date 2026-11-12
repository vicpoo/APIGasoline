using APIGasoline.Domain.Exceptions;

namespace APIGasoline.Domain.Entities;

/// <summary>
/// Corporativo de una empresa. La empresa es la raíz: tiene sus corporativos, sus divisiones
/// (relacionadas N:M con los corporativos por <see cref="CorporativoDivision"/>) y sus centros de costo.
/// </summary>
public class Corporativo
{
    public const int NombreMaxLength = 150;

    public Guid CorporativoId { get; private set; } = Guid.CreateVersion7();

    /// <summary>Empresa dueña del corporativo. No cambia.</summary>
    public Guid EmpresaId { get; private set; }

    public string Nombre { get; private set; } = string.Empty;
    public bool Activo { get; private set; } = true;

   
    public DateTime? FechaRegistro { get; private set; }

    // Requerido por Entity Framework
    private Corporativo()
    {
    }

    public static Corporativo Crear(Guid empresaId, string nombre, bool activo = true)
    {
        if (empresaId == Guid.Empty)
            throw new DomainException("El id de la empresa es obligatorio.");

        var corporativo = new Corporativo { EmpresaId = empresaId };
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
