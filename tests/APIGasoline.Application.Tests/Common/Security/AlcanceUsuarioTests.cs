using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Application.Common.Security;
using APIGasoline.Application.Tests.Features.Auth;

namespace APIGasoline.Application.Tests.Common.Security;

/// <summary>"Lo mío, o lo de otros con alguno de estos privilegios".</summary>
public class AlcanceUsuarioTests
{
    private static readonly Guid Usuario = Guid.Parse("01920000-0000-7000-8000-000000000001");
    private static readonly string[] Supervision = [Permisos.ViajeVerTodos, Permisos.AuditoriaVer];

    [Fact]
    public async Task LoPropio_PasaSinPrivilegios()
        => await new UsuarioActualFalso(Usuario, null).ValidarPropioOAlgunPrivilegioAsync(true, Supervision, "x");

    [Theory]
    [InlineData(Permisos.ViajeVerTodos)]
    [InlineData(Permisos.AuditoriaVer)]
    public async Task LoDeOtro_PasaConCualquieraDeLosPrivilegios(string privilegio)
        => await new UsuarioActualFalso(Usuario, null, privilegio).ValidarPropioOAlgunPrivilegioAsync(false, Supervision, "x");

    [Fact]
    public async Task LoDeOtro_SinPrivilegio_Lanza403()
        => await Assert.ThrowsAsync<ForbiddenException>(() =>
            new UsuarioActualFalso(Usuario, null, Permisos.ViajeVerPropios).ValidarPropioOAlgunPrivilegioAsync(false, Supervision, "x"));

    [Fact]
    public async Task SinSesion_Lanza401()
        => await Assert.ThrowsAsync<UnauthorizedException>(() =>
            new UsuarioActualFalso(null, null).ValidarPropioOAlgunPrivilegioAsync(true, Supervision, "x"));
}
