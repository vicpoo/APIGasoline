using APIGasoline.Domain.Exceptions;

namespace APIGasoline.Domain.Entities;


public class Privilegio
{
    public const int NombreMaxLength = 100;
    public const int DescripcionMaxLength = 255;

    public int PrivilegioId { get; private set; }
    public string Nombre { get; private set; } = string.Empty;
    public string? Descripcion { get; private set; }
    public bool Activo { get; private set; } = true;

    // Requerido por Entity Framework
    private Privilegio()
    {
    }

    public static Privilegio Crear(string nombre, string? descripcion, bool activo = true)
    {
        var privilegio = new Privilegio();
        privilegio.CambiarNombre(nombre);
        privilegio.CambiarDescripcion(descripcion);
        privilegio.Activo = activo;

        return privilegio;
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

    public void Activar() => Activo = true;

    public void Desactivar() => Activo = false;
}
