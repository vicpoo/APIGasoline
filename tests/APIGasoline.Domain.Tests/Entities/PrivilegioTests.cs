using APIGasoline.Domain.Entities;
using APIGasoline.Domain.Enums;
using APIGasoline.Domain.Exceptions;

namespace APIGasoline.Domain.Tests.Entities;

/// <summary>
/// Reglas del privilegio: clave inmutable con formato modulo.accion (se guarda en minúsculas),
/// módulo (por defecto el primer segmento de la clave), nombre visible obligatorio (máximo 100),
/// descripción opcional, ámbito (plataforma solo para roles globales) y activación.
/// </summary>
public class PrivilegioTests
{
    // ---------- Clave ----------

    [Fact]
    public void Crear_ClaveConEspaciosYMayusculas_SeNormaliza()
    {
        var privilegio = Privilegio.Crear("  Carro.Crear  ", "Agregar carro", null);

        Assert.Equal("carro.crear", privilegio.Clave);
    }

    [Theory]
    [InlineData("carro.crear")]
    [InlineData("viaje.ver_todos")]
    [InlineData("auditoria.revision.cerrar")]
    [InlineData("carga2.registrar")]
    public void Crear_ClaveValida_SeAcepta(string clave)
    {
        var privilegio = Privilegio.Crear(clave, "Nombre", null);

        Assert.Equal(clave, privilegio.Clave);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("carro")]            // falta la acción
    [InlineData("carro.")]
    [InlineData(".crear")]
    [InlineData("carro..crear")]
    [InlineData("carro.crear-nuevo")] // guion medio
    [InlineData("carro crear")]
    [InlineData("1carro.crear")]     // empieza con número
    [InlineData("carro.agregar_ñ")]  // fuera de [a-z0-9_]
    public void Crear_ClaveInvalida_LanzaDomainException(string clave)
    {
        Assert.Throws<DomainException>(() => Privilegio.Crear(clave, "Nombre", null));
    }

    [Fact]
    public void Crear_ClaveMayorA100_LanzaDomainException()
    {
        var clave = "carro." + new string('x', 95);

        Assert.Throws<DomainException>(() => Privilegio.Crear(clave, "Nombre", null));
    }

    // ---------- Módulo ----------

    [Fact]
    public void Crear_SinModulo_TomaElPrimerSegmentoDeLaClave()
    {
        var privilegio = Privilegio.Crear("viaje.ver_todos", "Ver todos los viajes", null);

        Assert.Equal("viaje", privilegio.Modulo);
    }

    [Fact]
    public void Crear_ConModulo_SeNormalizaAMinusculas()
    {
        var privilegio = Privilegio.Crear("solicitud.aprobar", "Aprobar solicitud", null, modulo: "  Reservas ");

        Assert.Equal("reservas", privilegio.Modulo);
    }

    [Theory]
    [InlineData("carro.crear")] // con punto
    [InlineData("mi modulo")]
    public void Crear_ModuloInvalido_LanzaDomainException(string modulo)
    {
        Assert.Throws<DomainException>(() => Privilegio.Crear("carro.crear", "Nombre", null, modulo: modulo));
    }

    [Fact]
    public void Crear_ModuloMayorA50_LanzaDomainException()
    {
        Assert.Throws<DomainException>(() => Privilegio.Crear("carro.crear", "Nombre", null, modulo: new string('x', 51)));
    }

    [Fact]
    public void CambiarModulo_Nulo_VuelveAlPrimerSegmentoDeLaClave()
    {
        var privilegio = Privilegio.Crear("carro.crear", "Agregar carro", null, modulo: "flota");

        privilegio.CambiarModulo(null);

        Assert.Equal("carro", privilegio.Modulo);
        Assert.Equal("carro.crear", privilegio.Clave);
    }

    // ---------- Nombre y descripción ----------

    [Fact]
    public void Crear_NombreConEspacios_SeRecorta()
    {
        var privilegio = Privilegio.Crear("carro.crear", "  Agregar carro  ", null);

        Assert.Equal("Agregar carro", privilegio.Nombre);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Crear_SinNombre_LanzaDomainException(string nombre)
    {
        Assert.Throws<DomainException>(() => Privilegio.Crear("carro.crear", nombre, null));
    }

    [Fact]
    public void Crear_NombreMayorAlMaximo_LanzaDomainException()
    {
        Assert.Throws<DomainException>(() => Privilegio.Crear("carro.crear", new string('x', 101), null));
    }

    [Fact]
    public void CambiarNombre_NoCambiaLaClave()
    {
        var privilegio = Privilegio.Crear("carro.crear", "Agregar carro", null);

        privilegio.CambiarNombre("Registrar carro");

        Assert.Equal("Registrar carro", privilegio.Nombre);
        Assert.Equal("carro.crear", privilegio.Clave);
    }

    [Fact]
    public void Crear_DescripcionEnBlanco_SeGuardaComoNull()
    {
        var privilegio = Privilegio.Crear("solicitud.crear", "Reservar carro", "   ");

        Assert.Null(privilegio.Descripcion);
    }

    [Fact]
    public void Crear_DescripcionMayorA255_LanzaDomainException()
    {
        Assert.Throws<DomainException>(() => Privilegio.Crear("solicitud.crear", "Reservar carro", new string('x', 256)));
    }

    // ---------- Ámbito ----------

    [Fact]
    public void Crear_SinAmbito_EsDeEmpresa()
    {
        var privilegio = Privilegio.Crear("carro.crear", "Agregar carro", null);

        Assert.Equal(AmbitoPrivilegio.Empresa, privilegio.Ambito);
    }

    [Fact]
    public void Crear_AmbitoInvalido_LanzaDomainException()
    {
        Assert.Throws<DomainException>(() => Privilegio.Crear("carro.crear", "Agregar carro", null, (AmbitoPrivilegio)99));
    }

    [Theory]
    [InlineData(AmbitoPrivilegio.Empresa, true, true)]
    [InlineData(AmbitoPrivilegio.Empresa, false, true)]
    [InlineData(AmbitoPrivilegio.Plataforma, true, true)]
    [InlineData(AmbitoPrivilegio.Plataforma, false, false)]
    public void AsignableA_SegunAmbitoYTipoDeRol(AmbitoPrivilegio ambito, bool rolGlobal, bool esperado)
    {
        var privilegio = Privilegio.Crear("empresa.crear", "Crear empresa", null, ambito);
        var rol = rolGlobal
            ? Rol.Crear("Administrador", null)
            : Rol.Crear("Supervisor", null, empresaId: Id(1));

        Assert.Equal(esperado, privilegio.AsignableA(rol));
    }

    // ---------- Activación ----------

    [Fact]
    public void Desactivar_Activo_QuedaInactivo()
    {
        var privilegio = Privilegio.Crear("carro.crear", "Agregar carro", null);

        privilegio.Desactivar();

        Assert.False(privilegio.Activo);
    }
}
