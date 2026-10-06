using APIGasoline.Domain.Entities;
using APIGasoline.Domain.Enums;
using APIGasoline.Domain.Exceptions;

namespace APIGasoline.Domain.Tests.Entities;

/// <summary>
/// Reglas del usuario: email normalizado (minúsculas, sin espacios) para que el login no dependa del formato,
/// hash de contraseña obligatorio y estado activo/inactivo.
/// </summary>
public class UsuarioTests
{
    private const string Hash = "PBKDF2$100000$c2FsdA==$aGFzaA==";

    [Fact]
    public void Crear_EmailConMayusculasYEspacios_SeNormaliza()
    {
        var usuario = ConstruirUsuario(email: "  Vic@Ejemplo.COM ");

        Assert.Equal("vic@ejemplo.com", usuario.Email);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Crear_SinEmail_LanzaDomainException(string email)
    {
        Assert.Throws<DomainException>(() => ConstruirUsuario(email: email));
    }

    [Fact]
    public void Crear_EmailMayorA150_LanzaDomainException()
    {
        var email = new string('a', 140) + "@ejemplo.com";  // 152 caracteres

        Assert.Throws<DomainException>(() => ConstruirUsuario(email: email));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Crear_SinHash_LanzaDomainException(string hash)
    {
        Assert.Throws<DomainException>(() => ConstruirUsuario(hash: hash));
    }

    [Fact]
    public void Crear_HashMayorA255_LanzaDomainException()
    {
        Assert.Throws<DomainException>(() => ConstruirUsuario(hash: new string('x', 256)));
    }

    // --- Estado ---

    [Fact]
    public void Crear_EstadoPorDefecto_EstaActivo()
    {
        var usuario = ConstruirUsuario();

        Assert.True(usuario.EstaActivo);
    }

    [Fact]
    public void Desactivar_UsuarioActivo_YaNoEstaActivo()
    {
        var usuario = ConstruirUsuario();

        usuario.Desactivar();

        Assert.Equal(EstadoUsuario.Inactivo, usuario.Estado);
        Assert.False(usuario.EstaActivo);
    }

    [Fact]
    public void Activar_UsuarioInactivo_EstaActivo()
    {
        var usuario = ConstruirUsuario(estado: EstadoUsuario.Inactivo);

        usuario.Activar();

        Assert.True(usuario.EstaActivo);
    }

    [Fact]
    public void CambiarEstado_ValorFueraDelEnum_LanzaDomainException()
    {
        var usuario = ConstruirUsuario();

        Assert.Throws<DomainException>(() => usuario.CambiarEstado((EstadoUsuario)99));
    }

    private static Usuario ConstruirUsuario(
        string email = "vic@ejemplo.com",
        string hash = Hash,
        EstadoUsuario estado = EstadoUsuario.Activo)
        => Usuario.Crear(email, hash, estado);
}
