using APIGasoline.Domain.Exceptions;

namespace APIGasoline.Domain.Entities;


public class Empresa
{
    public const int NombreMaxLength = 150;

    public Guid EmpresaId { get; private set; } = Guid.CreateVersion7();
    public Guid? CorporativoId { get; private set; }
    public Guid? DivisionId { get; private set; }
    public string Nombre { get; private set; } = string.Empty;


    /// <summary>Empresa activa o inactiva (HU-ADM-01). No es lo mismo que eliminada.</summary>
    public bool Activo { get; private set; } = true;

    public bool Eliminado { get; private set; }

    public DateTime? FechaRegistro { get; private set; }

    // Requerido por Entity Framework
    private Empresa()
    {
    }

    public static Empresa Crear(string nombre, Guid? corporativoId, Guid? divisionId)
    {
        var empresa = new Empresa();
        empresa.CambiarNombre(nombre);
        empresa.AsignarOrganizacion(corporativoId, divisionId);

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


    public void AsignarOrganizacion(Guid? corporativoId, Guid? divisionId)
    {
        if (corporativoId == Guid.Empty)
            throw new DomainException("El id del corporativo es obligatorio.");

        if (divisionId == Guid.Empty)
            throw new DomainException("El id de la división es obligatorio.");

        if (divisionId is not null && corporativoId is null)
            throw new DomainException("Para asignar una división también debe indicarse el corporativo.");

        CorporativoId = corporativoId;
        DivisionId = divisionId;
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
