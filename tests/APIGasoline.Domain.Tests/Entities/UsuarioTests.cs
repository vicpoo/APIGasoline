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

    // ---- Empresa de la cuenta (aislamiento entre empresas)

    private static readonly Guid EmpresaA = Guid.Parse("01920000-0000-7000-8000-00000000000a");
    private static readonly Guid EmpresaB = Guid.Parse("01920000-0000-7000-8000-00000000000b");

    [Fact]
    public void Crear_SinEmpresa_QuedaSinAsignar()
        => Assert.Null(ConstruirUsuario().EmpresaId);

    [Fact]
    public void Crear_ConEmpresa_QuedaEnEsaEmpresa()
    {
        var usuario = Usuario.Crear("vic@ejemplo.com", Hash, empresaId: EmpresaA);

        Assert.Equal(EmpresaA, usuario.EmpresaId);
        Assert.True(usuario.PerteneceA(EmpresaA));
        Assert.False(usuario.PerteneceA(EmpresaB));
    }

    [Fact]
    public void Crear_ConEmpresaVacia_LanzaDomainException()
        => Assert.Throws<DomainException>(() => Usuario.Crear("vic@ejemplo.com", Hash, empresaId: Guid.Empty));

    [Fact]
    public void AsignarEmpresa_SinEmpresa_LaAsigna()
    {
        var usuario = ConstruirUsuario();

        usuario.AsignarEmpresa(EmpresaA);

        Assert.Equal(EmpresaA, usuario.EmpresaId);
    }

    [Fact]
    public void AsignarEmpresa_LaMisma_NoCambiaNada()
    {
        var usuario = Usuario.Crear("vic@ejemplo.com", Hash, empresaId: EmpresaA);

        usuario.AsignarEmpresa(EmpresaA);

        Assert.Equal(EmpresaA, usuario.EmpresaId);
    }

    [Fact]
    public void AsignarEmpresa_OtraEmpresa_LanzaDomainException()
    {
        var usuario = Usuario.Crear("vic@ejemplo.com", Hash, empresaId: EmpresaA);

        Assert.Throws<DomainException>(() => usuario.AsignarEmpresa(EmpresaB));
        Assert.Equal(EmpresaA, usuario.EmpresaId);
    }

    [Fact]
    public void AsignarEmpresa_Vacia_LanzaDomainException()
        => Assert.Throws<DomainException>(() => ConstruirUsuario().AsignarEmpresa(Guid.Empty));

    private static Usuario ConstruirUsuario(
        string email = "vic@ejemplo.com",
        string hash = Hash,
        EstadoUsuario estado = EstadoUsuario.Activo)
        => Usuario.Crear(email, hash, estado);
}
