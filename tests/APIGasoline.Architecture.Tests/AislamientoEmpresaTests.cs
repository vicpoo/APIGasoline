using System.Reflection;
using APIGasoline.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace APIGasoline.Architecture.Tests;

/// <summary>
/// Que ninguna tabla quede fuera del aislamiento entre empresas:
/// - Cada tabla de negocio tiene el filtro global "Empresa" (lecturas).
/// - El guardián de SaveChanges sabe de qué empresa es cada una (escrituras).
/// - Nadie quita TODOS los filtros (IgnoreQueryFilters() sin nombre) salvo donde ya se revisó.
/// Si agregas una tabla, estas pruebas te obligan a decidir cómo se aísla.
/// </summary>
public sealed class AislamientoEmpresaTests
{
    private static readonly IModel Modelo = new AppDbContext(
        new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer("Server=sin-conexion;Database=solo-modelo")
            .Options).Model;

    private static readonly IReadOnlySet<Type> SinFiltroPermitido = AislamientoEmpresaInterceptor.Exentas
        .Concat(AislamientoEmpresaInterceptor.SoloPlataforma)
        .ToHashSet();

    /// <summary>Clases revisadas a mano que pueden quitar todos los filtros (solo para sí/no o para el guardián).</summary>
    private static readonly string[] PuedenQuitarTodosLosFiltros =
    [
        "EmpleadoRepository",            // UsuarioAsignadoAsync: el índice único es de toda la BD
        "EmpresaRepository",             // ExistsByCorporativo/Division: solo la plataforma borra corporativos y divisiones
        "AislamientoEmpresaInterceptor", // necesita saber de qué empresa es el registro ajeno para rechazarlo
    ];

    private static IEnumerable<IEntityType> Tablas => Modelo.GetEntityTypes().Where(t => !t.IsOwned());

    [Fact]
    public void Se_encontraron_las_tablas()
        => Assert.True(Tablas.Count() >= 20, $"Solo se encontraron {Tablas.Count()} tablas en el modelo.");

    [Fact]
    public void Cada_tabla_de_negocio_tiene_el_filtro_por_empresa()
        => SinViolaciones(
            Tablas.Where(t => !SinFiltroPermitido.Contains(t.ClrType) && t.FindDeclaredQueryFilter(FiltrosConsulta.Empresa) is null),
            "no tiene el filtro global 'Empresa' (agrégalo en AppDbContext.AplicarFiltroPorEmpresa o, si es técnica o de la plataforma, en las listas del guardián)");

    [Fact]
    public void Las_tablas_exentas_existen_en_el_modelo()
    {
        var tipos = Tablas.Select(t => t.ClrType).ToHashSet();

        Assert.All(SinFiltroPermitido, t => Assert.Contains(t, tipos));
    }

    [Fact]
    public void El_guardian_sabe_de_que_empresa_es_cada_tabla_filtrada()
        => SinViolaciones(
            Tablas.Where(t => t.FindDeclaredQueryFilter(FiltrosConsulta.Empresa) is not null
                              && t.FindProperty("EmpresaId") is null
                              && !AislamientoEmpresaInterceptor.PorRelacion.Contains(t.ClrType)),
            "tiene filtro por empresa pero no columna EmpresaId: agrégala a AislamientoEmpresaInterceptor.PorRelacion con su regla");

    [Fact]
    public void Solo_las_clases_revisadas_quitan_todos_los_filtros()
    {
        var malas = Ensamblados.Infrastructure.GetTypes()
            .Where(t => !t.IsNested && !PuedenQuitarTodosLosFiltros.Contains(t.Name))
            .Where(t => LectorIL.MiembrosUsados(t).Any(uso => EsIgnoreQueryFiltersSinNombre(uso.Miembro)))
            .Select(Ensamblados.Nombre)
            .ToList();

        Assert.True(malas.Count == 0,
            "Estas clases llaman IgnoreQueryFilters() sin nombre (quita también el filtro por empresa). " +
            $"Usa IgnoreQueryFilters([FiltrosConsulta.Eliminado]) o revísalo y agrégalo a la lista:\n  - {string.Join("\n  - ", malas)}");
    }

    /// <summary>Control: si el lector de IL dejara de ver la llamada, la prueba de arriba pasaría sin revisar nada.</summary>
    [Fact]
    public void Control_el_lector_de_IL_ve_IgnoreQueryFilters_sin_nombre()
        => Assert.Contains(LectorIL.MiembrosUsados(typeof(APIGasoline.Infrastructure.Persistence.Repositories.EmpleadoRepository)),
            uso => EsIgnoreQueryFiltersSinNombre(uso.Miembro));

    private static bool EsIgnoreQueryFiltersSinNombre(MemberInfo miembro)
        => miembro is MethodInfo { Name: nameof(EntityFrameworkQueryableExtensions.IgnoreQueryFilters) } metodo
           && metodo.DeclaringType == typeof(EntityFrameworkQueryableExtensions)
           && metodo.GetParameters().Length == 1;

    private static void SinViolaciones(IEnumerable<IEntityType> tablas, string regla)
    {
        var malas = tablas.Select(t => t.ClrType.Name).OrderBy(n => n).ToList();

        Assert.True(malas.Count == 0, $"{malas.Count} tabla(s): {regla}.\n  - {string.Join("\n  - ", malas)}");
    }
}
