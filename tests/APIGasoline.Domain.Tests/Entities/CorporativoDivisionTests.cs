using APIGasoline.Domain.Entities;
using APIGasoline.Domain.Exceptions;

namespace APIGasoline.Domain.Tests.Entities;

/// <summary>
/// Liga N:M corporativo ↔ división: solo entre registros de la misma empresa.
/// </summary>
public class CorporativoDivisionTests
{
    [Fact]
    public void Ligar_MismaEmpresa_CopiaLosIds()
    {
        var corporativo = Corporativo.Crear(Id(1), "Corporativo Sur");
        var division = Division.Crear(Id(1), "División Norte");

        var liga = CorporativoDivision.Ligar(corporativo, division);

        Assert.Equal(Id(1), liga.EmpresaId);
        Assert.Equal(corporativo.CorporativoId, liga.CorporativoId);
        Assert.Equal(division.DivisionId, liga.DivisionId);
    }

    [Fact]
    public void Ligar_EmpresasDistintas_LanzaDomainException()
    {
        var corporativo = Corporativo.Crear(Id(1), "Corporativo Sur");
        var division = Division.Crear(Id(2), "División Norte");

        Assert.Throws<DomainException>(() => CorporativoDivision.Ligar(corporativo, division));
    }

    [Fact]
    public void Ligar_CorporativoNulo_LanzaArgumentNullException()
    {
        var division = Division.Crear(Id(1), "División Norte");

        Assert.Throws<ArgumentNullException>(() => CorporativoDivision.Ligar(null!, division));
    }
}
