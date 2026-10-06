using APIGasoline.Domain.Entities;
using APIGasoline.Domain.Exceptions;

namespace APIGasoline.Domain.Tests.Entities;

/// <summary>
/// Asignación usuario-rol: ambos ids son obligatorios y se pueden reasignar.
/// </summary>
public class UsuarioRolTests
{
    [Fact]
    public void Reasignar_IdsValidos_CambiaLaAsignacion()
    {
        var asignacion = UsuarioRol.Crear(1, 1);

        asignacion.Reasignar(1, 2);

        Assert.Equal(2, asignacion.RolId);
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(1, 0)]
    public void Crear_IdsInvalidos_LanzaDomainException(int usuarioId, int rolId)
    {
        Assert.Throws<DomainException>(() => UsuarioRol.Crear(usuarioId, rolId));
    }
}
