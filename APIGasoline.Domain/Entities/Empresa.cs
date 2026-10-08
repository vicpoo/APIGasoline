using APIGasoline.Domain.Exceptions;

namespace APIGasoline.Domain.Entities;


public class Empresa
{
    public const int NombreMaxLength = 150;

    public Guid EmpresaId { get; private set; }
    public int? CorporativoId { get; private set; }
    public int? DivisionId { get; private set; }
    public string Nombre { get; private set; } = string.Empty;


    public bool Eliminado { get; private set; }

    public DateTime? FechaRegistro { get; private set; }

    // Requerido por Entity Framework
    private Empresa()
    {
    }

    public static Empresa Crear(string nombre, int? corporativoId, int? divisionId)
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


    public void AsignarOrganizacion(int? corporativoId, int? divisionId)
    {
        if (corporativoId is <= 0)
            throw new DomainException("El id del corporativo debe ser mayor a cero.");

        if (divisionId is <= 0)
            throw new DomainException("El id de la división debe ser mayor a cero.");

        if (divisionId is not null && corporativoId is null)
            throw new DomainException("Para asignar una división también debe indicarse el corporativo.");

        CorporativoId = corporativoId;
        DivisionId = divisionId;
    }

    public void Eliminar()
    {
        if (Eliminado)
            throw new DomainException("La empresa ya está eliminada.");

        Eliminado = true;
    }
}
