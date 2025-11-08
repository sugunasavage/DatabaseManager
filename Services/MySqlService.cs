using DatabaseManager.Models;
using MySqlConnector;
using System.Data;
using System.Diagnostics;

namespace DatabaseManager.Services;

public class MySqlService : IDatabaseService
{
    public async Task<bool> TestConnectionAsync(ConnectionInfo connectionInfo)
    {
        try
        {
            using var connection = new MySqlConnection(connectionInfo.GetConnectionString());
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
            using var connection = new MySqlConnection(connectionInfo.GetConnectionString());
            await connection.OpenAsync();

            using var command = new MySqlCommand(query, connection);
            command.CommandTimeout = 300;

            if (query.TrimStart().StartsWith("SELECT", StringComparison.OrdinalIgnoreCase) ||
                query.TrimStart().StartsWith("WITH", StringComparison.OrdinalIgnoreCase))
            {
                using var reader = await command.ExecuteReaderAsync();
                var dataTable = new DataTable();
                dataTable.Load(reader);
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
            using var connection = new MySqlConnection(connectionInfo.GetConnectionString());
            await connection.OpenAsync();

            using var command = new MySqlCommand(
                "SELECT schema_name FROM information_schema.schemata WHERE schema_name NOT IN ('mysql', 'information_schema', 'performance_schema', 'sys') ORDER BY schema_name",
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

            using var connection = new MySqlConnection(connInfo.GetConnectionString());
            await connection.OpenAsync();

            using var command = new MySqlCommand(
                "SELECT TABLE_SCHEMA, TABLE_NAME FROM information_schema.tables WHERE TABLE_TYPE = 'BASE TABLE' AND TABLE_SCHEMA = @database ORDER BY TABLE_NAME",
                connection);
            command.Parameters.AddWithValue("@database", database);
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

            using var connection = new MySqlConnection(connInfo.GetConnectionString());
            await connection.OpenAsync();

            using var command = new MySqlCommand(
                "SELECT TABLE_SCHEMA, TABLE_NAME FROM information_schema.views WHERE TABLE_SCHEMA = @database ORDER BY TABLE_NAME",
                connection);
            command.Parameters.AddWithValue("@database", database);
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

            using var connection = new MySqlConnection(connInfo.GetConnectionString());
            await connection.OpenAsync();

            using var command = new MySqlCommand(
                "SELECT ROUTINE_SCHEMA, ROUTINE_NAME FROM information_schema.routines WHERE ROUTINE_SCHEMA = @database ORDER BY ROUTINE_NAME",
                connection);
            command.Parameters.AddWithValue("@database", database);
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                procedures.Add(new DatabaseObject
                {
                    Schema = reader.GetString(0),
                    Name = reader.GetString(1),
                    Type = DatabaseObjectType.StoredProcedure
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

            using var connection = new MySqlConnection(connInfo.GetConnectionString());
            await connection.OpenAsync();

            using var command = new MySqlCommand(
                @"SELECT COLUMN_NAME, DATA_TYPE
                  FROM information_schema.columns
                  WHERE TABLE_NAME = @tableName AND TABLE_SCHEMA = @database
                  ORDER BY ORDINAL_POSITION",
                connection);
            command.Parameters.AddWithValue("@tableName", tableName);
            command.Parameters.AddWithValue("@database", database);
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
