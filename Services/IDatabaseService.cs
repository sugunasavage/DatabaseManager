using DatabaseManager.Models;

namespace DatabaseManager.Services;

public interface IDatabaseService
{
    Task<bool> TestConnectionAsync(ConnectionInfo connectionInfo);
    Task<QueryResult> ExecuteQueryAsync(ConnectionInfo connectionInfo, string query);
    Task<List<string>> GetDatabasesAsync(ConnectionInfo connectionInfo);
    Task<List<DatabaseObject>> GetTablesAsync(ConnectionInfo connectionInfo, string database);
    Task<List<DatabaseObject>> GetViewsAsync(ConnectionInfo connectionInfo, string database);
    Task<List<DatabaseObject>> GetStoredProceduresAsync(ConnectionInfo connectionInfo, string database);
    Task<List<DatabaseObject>> GetColumnsAsync(ConnectionInfo connectionInfo, string database, string tableName);
}
