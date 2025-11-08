using DatabaseManager.Models;
using Microsoft.Data.Sqlite;
using System.Data;
using System.Diagnostics;

namespace DatabaseManager.Services;

public class SqliteService : IDatabaseService
{
    public async Task<bool> TestConnectionAsync(ConnectionInfo connectionInfo)
    {
        try
        {
            using var connection = new SqliteConnection(connectionInfo.GetConnectionString());
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
            using var connection = new SqliteConnection(connectionInfo.GetConnectionString());
            await connection.OpenAsync();

            using var command = new SqliteCommand(query, connection);
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

    public Task<List<string>> GetDatabasesAsync(ConnectionInfo connectionInfo)
    {
        // SQLite only has one database per file
        return Task.FromResult(new List<string> { "main" });
    }

    public async Task<List<DatabaseObject>> GetTablesAsync(ConnectionInfo connectionInfo, string database)
    {
        var tables = new List<DatabaseObject>();
        try
        {
            using var connection = new SqliteConnection(connectionInfo.GetConnectionString());
            await connection.OpenAsync();

            using var command = new SqliteCommand(
                "SELECT name FROM sqlite_master WHERE type='table' AND name NOT LIKE 'sqlite_%' ORDER BY name",
                connection);
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                tables.Add(new DatabaseObject
                {
                    Name = reader.GetString(0),
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
            using var connection = new SqliteConnection(connectionInfo.GetConnectionString());
            await connection.OpenAsync();

            using var command = new SqliteCommand(
                "SELECT name FROM sqlite_master WHERE type='view' ORDER BY name",
                connection);
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                views.Add(new DatabaseObject
                {
                    Name = reader.GetString(0),
                    Type = DatabaseObjectType.View
                });
            }
        }
        catch { }

        return views;
    }

    public Task<List<DatabaseObject>> GetStoredProceduresAsync(ConnectionInfo connectionInfo, string database)
    {
        // SQLite doesn't support stored procedures
        return Task.FromResult(new List<DatabaseObject>());
    }

    public async Task<List<DatabaseObject>> GetColumnsAsync(ConnectionInfo connectionInfo, string database, string tableName)
    {
        var columns = new List<DatabaseObject>();
        try
        {
            using var connection = new SqliteConnection(connectionInfo.GetConnectionString());
            await connection.OpenAsync();

            using var command = new SqliteCommand($"PRAGMA table_info({tableName})", connection);
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                columns.Add(new DatabaseObject
                {
                    Name = $"{reader.GetString(1)} ({reader.GetString(2)})",
                    Type = DatabaseObjectType.Column
                });
            }
        }
        catch { }

        return columns;
    }
}
