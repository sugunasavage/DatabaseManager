using DatabaseManager.Models;

namespace DatabaseManager.Services;

public class DatabaseServiceFactory
{
    public static IDatabaseService GetService(DatabaseType databaseType)
    {
        return databaseType switch
        {
            DatabaseType.SqlServer => new SqlServerService(),
            DatabaseType.PostgreSQL => new PostgreSqlService(),
            DatabaseType.MySQL => new MySqlService(),
            DatabaseType.SQLite => new SqliteService(),
            _ => throw new NotSupportedException($"Database type {databaseType} is not supported")
        };
    }
}
