using APIGasoline.Domain.Exceptions;

namespace APIGasoline.Domain.Entities;


public class Division
{
    public const int NombreMaxLength = 100;

    public Guid DivisionId { get; private set; } = Guid.CreateVersion7();
    public Guid CorporativoId { get; private set; }
    public string Nombre { get; private set; } = string.Empty;
    public bool Activo { get; private set; } = true;

 
    public DateTime? FechaRegistro { get; private set; }

    // Requerido por Entity Framework
    private Division()
    {
    }

    public static Division Crear(Guid corporativoId, string nombre, bool activo = true)
    {
        var division = new Division();
        division.CambiarCorporativo(corporativoId);
        division.CambiarNombre(nombre);
        division.Activo = activo;

        return division;
    }

    public void CambiarCorporativo(Guid corporativoId)
    {
        if (corporativoId == Guid.Empty)
            throw new DomainException("El id del corporativo es obligatorio.");

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
