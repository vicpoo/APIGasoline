using APIGasoline.Domain.Exceptions;

namespace APIGasoline.Domain.Entities;


public class Empresa
{
    public const int NombreMaxLength = 150;

    public Guid EmpresaId { get; private set; } = Guid.CreateVersion7();
    public string Nombre { get; private set; } = string.Empty;


    /// <summary>Empresa activa o inactiva (HU-ADM-01). No es lo mismo que eliminada.</summary>
    public bool Activo { get; private set; } = true;

    public bool Eliminado { get; private set; }

    public DateTime? FechaRegistro { get; private set; }

    // Requerido por Entity Framework
    private Empresa()
    {
    }

    /// <summary>
    /// La empresa es la raíz de la organización: sus corporativos, divisiones y centros de costo
    /// apuntan a ella (no al revés).
    /// </summary>
    public static Empresa Crear(string nombre)
    {
        var empresa = new Empresa();
        empresa.CambiarNombre(nombre);

        return empresa;
    }

    public void CambiarNombre(string nombre)
    {
        var normalizado = nombre?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(normalizado))
            throw new DomainException("El nombre de la empresa es obligatorio.");

        if (normalizado.Length > NombreMaxLength)
            throw new DomainException($"El nombre de la empresa no puede exceder {NombreMaxLength} caracteres.");

        Nombre = normalizado;
    }


    public void Activar() => Activo = true;

    public void Desactivar() => Activo = false;

    public void Eliminar()
    {
        if (Eliminado)
            throw new DomainException("La empresa ya está eliminada.");

        Eliminado = true;
    }
}
