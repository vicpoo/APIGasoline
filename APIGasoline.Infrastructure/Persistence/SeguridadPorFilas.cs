namespace APIGasoline.Infrastructure.Persistence;

/// <summary>
/// Row-Level Security de SQL Server: el segundo candado del aislamiento entre empresas.
/// Aunque una consulta se salte el filtro de EF Core (SQL a mano, IgnoreQueryFilters, un error),
/// SQL Server tampoco devuelve ni deja escribir filas de otra empresa.
///
/// La API manda en cada conexión (ver <see cref="ContextoSesionSqlInterceptor"/>) dos valores de
/// SESSION_CONTEXT y la política <c>rls.aislamiento_empresa</c> (migración SeguridadPorFilas) los usa.
/// Una conexión que no los manda (SSMS, Azure Data Studio, otro programa) NO ve ninguna fila de
/// esas tablas: para consultar a mano hay que ejecutar antes
/// <c>EXEC sys.sp_set_session_context N'apigasoline.ve_todas', 1;</c>
/// </summary>
public static class SeguridadPorFilas
{
    /// <summary>binary(16): empresa a la que se limita la conexión.</summary>
    public const string ClaveEmpresa = "apigasoline.empresa_id";

    /// <summary>bit: 1 = la conexión ve todas las empresas (plataforma o procesos del sistema).</summary>
    public const string ClaveVeTodas = "apigasoline.ve_todas";

    /// <summary>Esquema de las funciones de predicado y de la política.</summary>
    public const string Esquema = "rls";

    public const string Politica = "rls.aislamiento_empresa";
}
