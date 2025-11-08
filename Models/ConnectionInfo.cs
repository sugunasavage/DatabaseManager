namespace DatabaseManager.Models;

public class ConnectionInfo
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public DatabaseType DatabaseType { get; set; }
    public string Server { get; set; } = string.Empty;
    public string? Port { get; set; }
    public string? Database { get; set; }
    public string? Username { get; set; }
    public string? Password { get; set; }
    public string? FilePath { get; set; } // For SQLite
    public bool UseIntegratedSecurity { get; set; }

    public string GetConnectionString()
    {
        return DatabaseType switch
        {
            DatabaseType.SqlServer => UseIntegratedSecurity
                ? $"Server={Server};Database={Database};Integrated Security=true;TrustServerCertificate=true;"
                : $"Server={Server};Database={Database};User Id={Username};Password={Password};TrustServerCertificate=true;",

            DatabaseType.PostgreSQL =>
                $"Host={Server};Port={Port ?? "5432"};Database={Database};Username={Username};Password={Password}",

            DatabaseType.MySQL =>
                $"Server={Server};Port={Port ?? "3306"};Database={Database};Uid={Username};Pwd={Password}",

            DatabaseType.SQLite =>
                $"Data Source={FilePath}",

            _ => throw new NotSupportedException($"Database type {DatabaseType} is not supported")
        };
    }
}
