using APIGasoline.Application.Common.Interfaces;
using APIGasoline.Application.Common.Security;
using APIGasoline.Domain.Enums;

namespace APIGasoline.Application.Tests.Common.Security;

/// <summary>De qué empresa son los datos de la petición (lo usan el filtro de EF y el guardián de SaveChanges).</summary>
public class ContextoEmpresaTests
{
    private static readonly Guid EmpresaA = Guid.Parse("01920000-0000-7000-8000-00000000000a");

    [Fact]
    public void SinUsuario_VeTodas()
    {
        var contexto = new ContextoEmpresa(new Usuario(autenticado: false));

        Assert.True(contexto.VeTodasLasEmpresas);
        Assert.Equal(Guid.Empty, contexto.EmpresaId);
    }

    [Fact]
    public void ConUsuario_SinInicializar_FallaCerrado()
    {
        var contexto = new ContextoEmpresa(new Usuario(empresaId: EmpresaA));

        Assert.Throws<InvalidOperationException>(() => contexto.VeTodasLasEmpresas);
        Assert.Throws<InvalidOperationException>(() => contexto.EmpresaId);
    }

    [Fact]
    public async Task UsuarioDeEmpresa_SoloSuEmpresa()
    {
        var contexto = new ContextoEmpresa(new Usuario(empresaId: EmpresaA));

        await contexto.InicializarAsync();

        Assert.False(contexto.VeTodasLasEmpresas);
        Assert.Equal(EmpresaA, contexto.EmpresaId);
    }

    [Fact]
    public async Task UsuarioSinEmpresa_NoVeNada()
    {
        var contexto = new ContextoEmpresa(new Usuario(empresaId: null));

        await contexto.InicializarAsync();

        Assert.False(contexto.VeTodasLasEmpresas);
        Assert.Equal(Guid.Empty, contexto.EmpresaId);
    }

    [Fact]
    public async Task Plataforma_VeTodas()
    {
        var contexto = new ContextoEmpresa(new Usuario(empresaId: EmpresaA, Permisos.PlataformaMultiempresa));

        await contexto.InicializarAsync();

        Assert.True(contexto.VeTodasLasEmpresas);
    }

    [Fact]
    public async Task MientrasLeeLosPrivilegios_VeTodas()
    {
        ContextoEmpresa? contexto = null;
        bool? veiaTodasDurante = null;
        var usuario = new Usuario(empresaId: EmpresaA) { AlLeerPrivilegios = () => veiaTodasDurante = contexto!.VeTodasLasEmpresas };
        contexto = new ContextoEmpresa(usuario);

        await contexto.InicializarAsync();

        Assert.True(veiaTodasDurante);
        Assert.False(contexto.VeTodasLasEmpresas);
    }

    [Fact]
    public async Task SiFallaAlInicializar_SigueCerrado()
    {
        var contexto = new ContextoEmpresa(new Usuario(empresaId: EmpresaA) { AlLeerPrivilegios = () => throw new TimeoutException() });

        await Assert.ThrowsAsync<TimeoutException>(() => contexto.InicializarAsync());

        Assert.Throws<InvalidOperationException>(() => contexto.VeTodasLasEmpresas);
    }

    private sealed class Usuario(Guid? empresaId = null, params string[] privilegios) : IUsuarioActual
    {
        public Usuario(bool autenticado) : this()
        {
            EstaAutenticado = autenticado;
        }

        public Action? AlLeerPrivilegios { get; init; }

        public bool EstaAutenticado { get; } = true;
        public Guid? UsuarioId => EstaAutenticado ? Guid.CreateVersion7() : null;
        public Guid? EmpleadoId => null;
        public Guid? EmpresaId => empresaId;
        public Guid? SesionId => null;
        public IReadOnlyCollection<string> Roles => [];
        public OrigenAuditoria Origen => OrigenAuditoria.Web;

        public Task<IReadOnlySet<string>> GetPrivilegiosAsync(CancellationToken cancellationToken = default)
        {
            AlLeerPrivilegios?.Invoke();
            return Task.FromResult<IReadOnlySet<string>>(new HashSet<string>(privilegios));
        }

        public async Task<bool> TienePrivilegioAsync(string clave, CancellationToken cancellationToken = default)
            => (await GetPrivilegiosAsync(cancellationToken)).Contains(clave);
    }
}
