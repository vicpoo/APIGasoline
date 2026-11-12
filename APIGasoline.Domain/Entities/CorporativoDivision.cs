using APIGasoline.Domain.Exceptions;

namespace APIGasoline.Domain.Entities;

/// <summary>
/// Liga N:M entre un corporativo y una división de la MISMA empresa.
/// Un centro de costo que tiene corporativo y división solo puede usar una pareja ligada.
/// </summary>
public class CorporativoDivision
{
    public Guid CorporativoDivisionId { get; private set; } = Guid.CreateVersion7();
    public Guid EmpresaId { get; private set; }
    public Guid CorporativoId { get; private set; }
    public Guid DivisionId { get; private set; }
    public DateTime? FechaRegistro { get; private set; }

    // Requerido por Entity Framework
    private CorporativoDivision()
    {
    }

    public static CorporativoDivision Ligar(Corporativo corporativo, Division division)
    {
        ArgumentNullException.ThrowIfNull(corporativo);
        ArgumentNullException.ThrowIfNull(division);

        if (corporativo.EmpresaId != division.EmpresaId)
            throw new DomainException("El corporativo y la división deben ser de la misma empresa.");

        return new CorporativoDivision
        {
            EmpresaId = corporativo.EmpresaId,
            CorporativoId = corporativo.CorporativoId,
            DivisionId = division.DivisionId,
        };
    }
}
