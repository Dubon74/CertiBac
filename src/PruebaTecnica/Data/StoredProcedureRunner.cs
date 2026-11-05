using System.Data;
using Microsoft.Data.SqlClient;

namespace TriviaApp.Data;

/// <summary>
/// Único punto de acceso a SQL Server. Solo sabe ejecutar procedimientos
/// almacenados (CommandType.StoredProcedure): la aplicación no contiene SQL.
/// </summary>
public interface IStoredProcedureRunner
{
    Task<List<T>> QueryAsync<T>(string procedure, Func<SqlDataReader, T> map, params SqlParameter[] parameters)
        where T : class;

    Task<T?> QuerySingleOrDefaultAsync<T>(string procedure, Func<SqlDataReader, T> map, params SqlParameter[] parameters)
        where T : class;
}

public sealed class SqlStoredProcedureRunner : IStoredProcedureRunner
{
    private readonly string _connectionString;

    public SqlStoredProcedureRunner(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<List<T>> QueryAsync<T>(string procedure, Func<SqlDataReader, T> map, params SqlParameter[] parameters)
        where T : class
    {
        await using var connection = new SqlConnection(_connectionString);
        await using var command = new SqlCommand(procedure, connection)
        {
            CommandType = CommandType.StoredProcedure,
            CommandTimeout = 30
        };

        if (parameters.Length > 0)
        {
            command.Parameters.AddRange(parameters);
        }

        await connection.OpenAsync();
        await using var reader = await command.ExecuteReaderAsync();

        var result = new List<T>();
        while (await reader.ReadAsync())
        {
            result.Add(map(reader));
        }

        return result;
    }

    public async Task<T?> QuerySingleOrDefaultAsync<T>(string procedure, Func<SqlDataReader, T> map, params SqlParameter[] parameters)
        where T : class
    {
        var rows = await QueryAsync(procedure, map, parameters);
        return rows.FirstOrDefault();
    }
}

/// <summary>Helpers para crear parámetros tipados (siempre parametrizados).</summary>
public static class Sql
{
    public static SqlParameter Param(string name, SqlDbType type, object? value, int size = 0)
    {
        var parameter = size > 0 ? new SqlParameter(name, type, size) : new SqlParameter(name, type);
        parameter.Value = value ?? DBNull.Value;
        return parameter;
    }
}

internal static class SqlDataReaderExtensions
{
    public static int Int(this SqlDataReader r, string column) => r.GetInt32(r.GetOrdinal(column));

    public static string Str(this SqlDataReader r, string column) => r.GetString(r.GetOrdinal(column));

    public static bool Bool(this SqlDataReader r, string column) => r.GetBoolean(r.GetOrdinal(column));

    public static int? NullableInt(this SqlDataReader r, string column)
    {
        var ordinal = r.GetOrdinal(column);
        return r.IsDBNull(ordinal) ? null : r.GetInt32(ordinal);
    }

    public static bool NullableBool(this SqlDataReader r, string column)
    {
        var ordinal = r.GetOrdinal(column);
        return !r.IsDBNull(ordinal) && r.GetBoolean(ordinal);
    }

    public static string? NullableStr(this SqlDataReader r, string column)
    {
        var ordinal = r.GetOrdinal(column);
        return r.IsDBNull(ordinal) ? null : r.GetString(ordinal);
    }
}
