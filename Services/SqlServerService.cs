using DatabaseManager.Models;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Diagnostics;

namespace DatabaseManager.Services;

public class SqlServerService : IDatabaseService
{
    public async Task<bool> TestConnectionAsync(ConnectionInfo connectionInfo)
    {
        try
        {
            using var connection = new SqlConnection(connectionInfo.GetConnectionString());
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
            using var connection = new SqlConnection(connectionInfo.GetConnectionString());
            await connection.OpenAsync();

            using var command = new SqlCommand(query, connection);
            command.CommandTimeout = 300; // 5 minutes

            if (query.TrimStart().StartsWith("SELECT", StringComparison.OrdinalIgnoreCase) ||
                query.TrimStart().StartsWith("WITH", StringComparison.OrdinalIgnoreCase))
            {
                using var adapter = new SqlDataAdapter(command);
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
            using var connection = new SqlConnection(connectionInfo.GetConnectionString());
            await connection.OpenAsync();

            using var command = new SqlCommand("SELECT name FROM sys.databases WHERE database_id > 4 ORDER BY name", connection);
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
                Database = database,
                Username = connectionInfo.Username,
                Password = connectionInfo.Password,
                UseIntegratedSecurity = connectionInfo.UseIntegratedSecurity
            };

            using var connection = new SqlConnection(connInfo.GetConnectionString());
            await connection.OpenAsync();

            using var command = new SqlCommand(
                "SELECT TABLE_SCHEMA, TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE' ORDER BY TABLE_SCHEMA, TABLE_NAME",
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
                Database = database,
                Username = connectionInfo.Username,
                Password = connectionInfo.Password,
                UseIntegratedSecurity = connectionInfo.UseIntegratedSecurity
            };

            using var connection = new SqlConnection(connInfo.GetConnectionString());
            await connection.OpenAsync();

            using var command = new SqlCommand(
                "SELECT TABLE_SCHEMA, TABLE_NAME FROM INFORMATION_SCHEMA.VIEWS ORDER BY TABLE_SCHEMA, TABLE_NAME",
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
                Database = database,
                Username = connectionInfo.Username,
                Password = connectionInfo.Password,
                UseIntegratedSecurity = connectionInfo.UseIntegratedSecurity
            };

            using var connection = new SqlConnection(connInfo.GetConnectionString());
            await connection.OpenAsync();

            using var command = new SqlCommand(
                "SELECT ROUTINE_SCHEMA, ROUTINE_NAME FROM INFORMATION_SCHEMA.ROUTINES WHERE ROUTINE_TYPE = 'PROCEDURE' ORDER BY ROUTINE_SCHEMA, ROUTINE_NAME",
                connection);
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
                Database = database,
                Username = connectionInfo.Username,
                Password = connectionInfo.Password,
                UseIntegratedSecurity = connectionInfo.UseIntegratedSecurity
            };

            using var connection = new SqlConnection(connInfo.GetConnectionString());
            await connection.OpenAsync();

            using var command = new SqlCommand(
                @"SELECT COLUMN_NAME, DATA_TYPE
                  FROM INFORMATION_SCHEMA.COLUMNS
                  WHERE TABLE_NAME = @tableName
                  ORDER BY ORDINAL_POSITION",
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
