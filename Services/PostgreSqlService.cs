using DatabaseManager.Models;
using Npgsql;
using System.Data;
using System.Diagnostics;

namespace DatabaseManager.Services;

public class PostgreSqlService : IDatabaseService
{
    public async Task<bool> TestConnectionAsync(ConnectionInfo connectionInfo)
    {
        try
        {
            using var connection = new NpgsqlConnection(connectionInfo.GetConnectionString());
            await connection.OpenAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<QueryResult> ExecuteQueryAsync(ConnectionInfo connectionInfo, string query)
    {
        var result = new QueryResult();
        var stopwatch = Stopwatch.StartNew();

        try
        {
            using var connection = new NpgsqlConnection(connectionInfo.GetConnectionString());
            await connection.OpenAsync();

            using var command = new NpgsqlCommand(query, connection);
            command.CommandTimeout = 300;

            if (query.TrimStart().StartsWith("SELECT", StringComparison.OrdinalIgnoreCase) ||
                query.TrimStart().StartsWith("WITH", StringComparison.OrdinalIgnoreCase))
            {
                using var adapter = new Npgsql.NpgsqlDataAdapter(command);
                var dataTable = new DataTable();
                adapter.Fill(dataTable);
                result.Data = dataTable;
                result.RowsAffected = dataTable.Rows.Count;
            }
            else
            {
                result.RowsAffected = await command.ExecuteNonQueryAsync();
            }
        }
        catch (Exception ex)
        {
            result.ErrorMessage = ex.Message;
        }
        finally
        {
            stopwatch.Stop();
            result.ExecutionTime = stopwatch.Elapsed;
        }

        return result;
    }

    public async Task<List<string>> GetDatabasesAsync(ConnectionInfo connectionInfo)
    {
        var databases = new List<string>();
        try
        {
            using var connection = new NpgsqlConnection(connectionInfo.GetConnectionString());
            await connection.OpenAsync();

            using var command = new NpgsqlCommand(
                "SELECT datname FROM pg_database WHERE datistemplate = false ORDER BY datname",
                connection);
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                databases.Add(reader.GetString(0));
            }
        }
        catch { }

        return databases;
    }

    public async Task<List<DatabaseObject>> GetTablesAsync(ConnectionInfo connectionInfo, string database)
    {
        var tables = new List<DatabaseObject>();
        try
        {
            var connInfo = new ConnectionInfo
            {
                DatabaseType = connectionInfo.DatabaseType,
                Server = connectionInfo.Server,
                Port = connectionInfo.Port,
                Database = database,
                Username = connectionInfo.Username,
                Password = connectionInfo.Password
            };

            using var connection = new NpgsqlConnection(connInfo.GetConnectionString());
            await connection.OpenAsync();

            using var command = new NpgsqlCommand(
                @"SELECT table_schema, table_name
                  FROM information_schema.tables
                  WHERE table_type = 'BASE TABLE' AND table_schema NOT IN ('pg_catalog', 'information_schema')
                  ORDER BY table_schema, table_name",
                connection);
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                tables.Add(new DatabaseObject
                {
                    Schema = reader.GetString(0),
                    Name = reader.GetString(1),
                    Type = DatabaseObjectType.Table
                });
            }
        }
        catch { }

        return tables;
    }

    public async Task<List<DatabaseObject>> GetViewsAsync(ConnectionInfo connectionInfo, string database)
    {
        var views = new List<DatabaseObject>();
        try
        {
            var connInfo = new ConnectionInfo
            {
                DatabaseType = connectionInfo.DatabaseType,
                Server = connectionInfo.Server,
                Port = connectionInfo.Port,
                Database = database,
                Username = connectionInfo.Username,
                Password = connectionInfo.Password
            };

            using var connection = new NpgsqlConnection(connInfo.GetConnectionString());
            await connection.OpenAsync();

            using var command = new NpgsqlCommand(
                @"SELECT table_schema, table_name
                  FROM information_schema.views
                  WHERE table_schema NOT IN ('pg_catalog', 'information_schema')
                  ORDER BY table_schema, table_name",
                connection);
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                views.Add(new DatabaseObject
                {
                    Schema = reader.GetString(0),
                    Name = reader.GetString(1),
                    Type = DatabaseObjectType.View
                });
            }
        }
        catch { }

        return views;
    }

    public async Task<List<DatabaseObject>> GetStoredProceduresAsync(ConnectionInfo connectionInfo, string database)
    {
        var procedures = new List<DatabaseObject>();
        try
        {
            var connInfo = new ConnectionInfo
            {
                DatabaseType = connectionInfo.DatabaseType,
                Server = connectionInfo.Server,
                Port = connectionInfo.Port,
                Database = database,
                Username = connectionInfo.Username,
                Password = connectionInfo.Password
            };

            using var connection = new NpgsqlConnection(connInfo.GetConnectionString());
            await connection.OpenAsync();

            using var command = new NpgsqlCommand(
                @"SELECT routine_schema, routine_name
                  FROM information_schema.routines
                  WHERE routine_type = 'FUNCTION' AND routine_schema NOT IN ('pg_catalog', 'information_schema')
                  ORDER BY routine_schema, routine_name",
                connection);
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                procedures.Add(new DatabaseObject
                {
                    Schema = reader.GetString(0),
                    Name = reader.GetString(1),
                    Type = DatabaseObjectType.Function
                });
            }
        }
        catch { }

        return procedures;
    }

    public async Task<List<DatabaseObject>> GetColumnsAsync(ConnectionInfo connectionInfo, string database, string tableName)
    {
        var columns = new List<DatabaseObject>();
        try
        {
            var connInfo = new ConnectionInfo
            {
                DatabaseType = connectionInfo.DatabaseType,
                Server = connectionInfo.Server,
                Port = connectionInfo.Port,
                Database = database,
                Username = connectionInfo.Username,
                Password = connectionInfo.Password
            };

            using var connection = new NpgsqlConnection(connInfo.GetConnectionString());
            await connection.OpenAsync();

            using var command = new NpgsqlCommand(
                @"SELECT column_name, data_type
                  FROM information_schema.columns
                  WHERE table_name = @tableName
                  ORDER BY ordinal_position",
                connection);
            command.Parameters.AddWithValue("@tableName", tableName);
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                columns.Add(new DatabaseObject
                {
                    Name = $"{reader.GetString(0)} ({reader.GetString(1)})",
                    Type = DatabaseObjectType.Column
                });
            }
        }
        catch { }

        return columns;
    }
}
