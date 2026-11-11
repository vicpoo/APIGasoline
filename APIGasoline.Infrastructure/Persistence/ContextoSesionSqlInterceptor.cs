using System.Data;
using System.Data.Common;
using APIGasoline.Application.Common.Interfaces;
using APIGasoline.Infrastructure.Persistence.Converters;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace APIGasoline.Infrastructure.Persistence;

/// <summary>
/// Cada vez que EF abre una conexión le dice a SQL Server de qué empresa es la petición
/// (SESSION_CONTEXT). Con eso la política de Row-Level Security filtra y bloquea filas aunque
/// la consulta no traiga el filtro de EF. Ver <see cref="SeguridadPorFilas"/>.
///
/// Se manda en CADA apertura porque el pool de conexiones limpia el contexto al reutilizarlas.
/// Si hay un usuario y su empresa todavía no se conoce, <see cref="IContextoEmpresa"/> lanza
/// excepción y la conexión no se usa (falla cerrado).
/// </summary>
public sealed class ContextoSesionSqlInterceptor : DbConnectionInterceptor
{
    private static readonly string Sql =
        $"EXEC sys.sp_set_session_context @key = N'{SeguridadPorFilas.ClaveEmpresa}', @value = @empresa; " +
        $"EXEC sys.sp_set_session_context @key = N'{SeguridadPorFilas.ClaveVeTodas}', @value = @veTodas;";

    private readonly IContextoEmpresa _contextoEmpresa;

    public ContextoSesionSqlInterceptor(IContextoEmpresa contextoEmpresa)
    {
        _contextoEmpresa = contextoEmpresa;
    }

    public override void ConnectionOpened(DbConnection connection, ConnectionEndEventData eventData)
    {
        using var comando = CrearComando(connection);
        comando.ExecuteNonQuery();
    }

    public override async Task ConnectionOpenedAsync(DbConnection connection, ConnectionEndEventData eventData, CancellationToken cancellationToken = default)
    {
        await using var comando = CrearComando(connection);
        await comando.ExecuteNonQueryAsync(cancellationToken);
    }

    private DbCommand CrearComando(DbConnection conexion)
    {
        var veTodas = _contextoEmpresa.VeTodasLasEmpresas;
        var empresa = veTodas ? Guid.Empty : _contextoEmpresa.EmpresaId;

        var comando = conexion.CreateCommand();
        comando.CommandText = Sql;

        var parametroEmpresa = comando.CreateParameter();
        parametroEmpresa.ParameterName = "@empresa";
        parametroEmpresa.DbType = DbType.Binary;
        parametroEmpresa.Size = 16;
        parametroEmpresa.Value = GuidBinarioConverter.ABytes(empresa);
        comando.Parameters.Add(parametroEmpresa);

        var parametroVeTodas = comando.CreateParameter();
        parametroVeTodas.ParameterName = "@veTodas";
        parametroVeTodas.DbType = DbType.Boolean;
        parametroVeTodas.Value = veTodas;
        comando.Parameters.Add(parametroVeTodas);

        return comando;
    }
}
