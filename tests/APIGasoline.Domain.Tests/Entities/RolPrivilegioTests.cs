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
        var asignacion = RolPrivilegio.Crear(1, 1);

        asignacion.Reasignar(1, 2);

        Assert.Equal(2, asignacion.PrivilegioId);
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(1, 0)]
    public void Crear_IdsInvalidos_LanzaDomainException(int rolId, int privilegioId)
    {
        Assert.Throws<DomainException>(() => RolPrivilegio.Crear(rolId, privilegioId));
    }
}
