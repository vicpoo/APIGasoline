using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;

namespace APIGasoline.Architecture.Tests;

/// <summary>
/// A009: desde la migración SeguridadPorFilas, las tablas de negocio tienen Row-Level Security. Las
/// migraciones corren con el login migrador y sin SESSION_CONTEXT, así que un UPDATE, DELETE o
/// INSERT ... SELECT sobre esas tablas afecta CERO filas sin dar error: la corrección de datos parece
/// aplicada y no lo está. Toda migración posterior que toque datos debe ejecutar antes
/// <c>EXEC sys.sp_set_session_context @key = N'apigasoline.ve_todas', @value = 1;</c>
/// (ver IndicesUnicosConcurrencia y BD/README.md).
/// </summary>
public sealed partial class MigracionesTests
{
    /// <summary>La primera migración con Row-Level Security.</summary>
    private const string PrimeraConRls = "20261111222631_SeguridadPorFilas";

    /// <summary>Revisadas a mano: tocan datos de tablas que en ese momento todavía no tenían Row-Level Security.</summary>
    private static readonly string[] Revisadas =
    [
        "20261112172840_EmpresaDesactivada", // sesion_usuario entró a la política hasta AislamientoTablasTecnicas
    ];

    public static TheoryData<string> Migraciones
    {
        get
        {
            var datos = new TheoryData<string>();
            foreach (var id in TiposDeMigracion().Keys.Where(id => string.CompareOrdinal(id, PrimeraConRls) > 0 && !Revisadas.Contains(id)))
                datos.Add(id);
            return datos;
        }
    }

    [Fact]
    public void Se_encontraron_las_migraciones()
        => Assert.True(TiposDeMigracion().Count >= 6, $"Solo se encontraron {TiposDeMigracion().Count} migraciones.");

    [Theory]
    [MemberData(nameof(Migraciones))]
    public void Si_una_migracion_toca_datos_primero_ve_todas_las_empresas(string id)
    {
        var migracion = (Migration)Activator.CreateInstance(TiposDeMigracion()[id])!;

        foreach (var (sentido, operaciones) in new[] { ("Up", migracion.UpOperations), ("Down", migracion.DownOperations) })
        {
            var tocaDatos = operaciones.Any(TocaDatos);
            var veTodas = operaciones.OfType<SqlOperation>().Any(o => o.Sql.Contains("apigasoline.ve_todas", StringComparison.OrdinalIgnoreCase)
                                                                   && o.Sql.Contains("sp_set_session_context", StringComparison.OrdinalIgnoreCase));

            Assert.False(tocaDatos && !veTodas,
                $"{id}.{sentido} modifica datos pero no ejecuta antes sp_set_session_context 'apigasoline.ve_todas' = 1: " +
                "con Row-Level Security afectaría cero filas sin error.");
        }
    }

    private static bool TocaDatos(MigrationOperation operacion) => operacion switch
    {
        InsertDataOperation or UpdateDataOperation or DeleteDataOperation => true,
        SqlOperation sql => SentenciaDeDatos().IsMatch(sql.Sql),
        _ => false,
    };

    /// <summary>"UPDATE x SET", "DELETE [alias] FROM", "INSERT INTO ... SELECT". No confunde "AFTER UPDATE" ni "DENY UPDATE, DELETE ON".</summary>
    [GeneratedRegex(@"\bUPDATE\s+(?:dbo\.)?\[?\w+\]?\s+SET\b|\bDELETE\s+(?:\w+\s+)?FROM\b|\bINSERT\s+INTO\b[^;]*?\bSELECT\b",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex SentenciaDeDatos();

    private static Dictionary<string, Type> TiposDeMigracion()
        => Ensamblados.Infrastructure.GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false } && typeof(Migration).IsAssignableFrom(t))
            .Select(t => (Id: t.GetCustomAttribute<MigrationAttribute>()?.Id, Tipo: t))
            .Where(x => x.Id is not null)
            .ToDictionary(x => x.Id!, x => x.Tipo);
}
