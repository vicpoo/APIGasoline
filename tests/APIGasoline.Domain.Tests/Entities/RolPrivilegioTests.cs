using APIGasoline.Domain.Entities;
using APIGasoline.Domain.Exceptions;

namespace APIGasoline.Domain.Tests.Entities;

/// <summary>
/// Asignación rol-privilegio: ambos ids son obligatorios y se pueden reasignar.
/// </summary>
public class RolPrivilegioTests
{
    [Fact]
    public void Reasignar_IdsValidos_CambiaLaAsignacion()
    {
        var asignacion = RolPrivilegio.Crear(Id(1), Id(1));

        asignacion.Reasignar(Id(1), Id(2));

        Assert.Equal(Id(2), asignacion.PrivilegioId);
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(1, 0)]
    public void Crear_IdsInvalidos_LanzaDomainException(int rolId, int privilegioId)
    {
        Assert.Throws<DomainException>(() => RolPrivilegio.Crear(Id(rolId), Id(privilegioId)));
    }
}
