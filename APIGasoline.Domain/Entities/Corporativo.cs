using APIGasoline.Domain.Exceptions;

namespace APIGasoline.Domain.Entities;


public class Corporativo
{
    public const int NombreMaxLength = 150;

    public Guid CorporativoId { get; private set; } = Guid.CreateVersion7();
    public string Nombre { get; private set; } = string.Empty;
    public bool Activo { get; private set; } = true;

   
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
