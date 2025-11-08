# DatabaseManager

A cross-platform SSMS-like database management tool built with .NET MAUI for macOS.

## Features

### Supported Databases
- ✅ Microsoft SQL Server
- ✅ PostgreSQL
- ✅ MySQL / MariaDB
- ✅ SQLite

### Core Functionality
- **Connection Manager**: Create, save, and manage multiple database connections
- **Object Explorer**: Browse database structures (databases, tables, views, stored procedures, columns)
- **Query Editor**: Write and execute SQL queries with monospace font
- **Result Grid**: View query results in a formatted grid with execution time
- **Multi-Database Support**: Switch between different database connections seamlessly

## Building & Running

### Prerequisites
- macOS with Apple Silicon (M-series) or Intel
- .NET 9 SDK
- Xcode 26.0 or later

### Build
```bash
cd DatabaseManager
dotnet build
```

### Run
```bash
dotnet run
```

Or for release mode:
```bash
dotnet build -c Release
dotnet run -c Release
```

## Usage

### Creating a Connection
1. Click **"New Connection"** button
2. Fill in connection details:
   - **Name**: A friendly name for your connection
   - **Database Type**: Select from SQL Server, PostgreSQL, MySQL, or SQLite
   - **Server**: Server hostname or IP address
   - **Database**: Database name
   - **Username**: Database username
   - **Password**: Database password

### Testing a Connection
1. Select a connection from the dropdown
2. Click **"Test Connection"**
3. Check the status bar for results

### Executing Queries
1. Select an active connection
2. Write your SQL query in the Query Editor
3. Click **"Execute (F5)"** or press F5
4. View results in the Results panel below
5. Check execution time in the bottom right

### Browsing Database Objects
1. Select a connection
2. Expand databases in the Object Explorer
3. Click on folders to browse:
   - Tables
   - Views
   - Stored Procedures
4. Expand tables/views to see columns

## Architecture

### Project Structure
```
DatabaseManager/
├── Models/             # Data models
│   ├── DatabaseType.cs
│   ├── ConnectionInfo.cs
│   ├── DatabaseObject.cs
│   └── QueryResult.cs
├── Services/           # Database services
│   ├── IDatabaseService.cs
│   ├── DatabaseServiceFactory.cs
│   ├── SqlServerService.cs
│   ├── PostgreSqlService.cs
│   ├── MySqlService.cs
│   └── SqliteService.cs
├── ViewModels/         # MVVM ViewModels
│   └── MainViewModel.cs
├── Views/              # UI Views
│   └── MainPage.xaml
└── Helpers/            # Value converters
    ├── IsNotNullConverter.cs
    ├── DataTableToStringConverter.cs
    └── EnumToArrayConverter.cs
```

### Design Patterns
- **MVVM**: Model-View-ViewModel pattern with CommunityToolkit.Mvvm
- **Factory Pattern**: DatabaseServiceFactory for database service creation
- **Dependency Injection**: Services registered in MauiProgram.cs
- **Service Abstraction**: IDatabaseService interface for database operations

## Dependencies

- **Microsoft.Maui.Controls**: Cross-platform UI framework
- **CommunityToolkit.Maui**: MAUI community extensions
- **CommunityToolkit.Mvvm**: MVVM helpers and commands
- **Microsoft.Data.SqlClient**: SQL Server connectivity
- **Npgsql**: PostgreSQL connectivity
- **MySqlConnector**: MySQL connectivity
- **Microsoft.Data.Sqlite**: SQLite connectivity

## Connection String Formats

### SQL Server
```
Server=localhost;Database=mydb;User Id=sa;Password=****;TrustServerCertificate=true;
```

### PostgreSQL
```
Host=localhost;Port=5432;Database=mydb;Username=postgres;Password=****
```

### MySQL
```
Server=localhost;Port=3306;Database=mydb;Uid=root;Pwd=****
```

### SQLite
```
Data Source=/path/to/database.db
```

## Known Issues

- Xcode 26.1 compatibility requires the `Directory.Build.targets` workaround
- XAML binding warnings (non-critical, performance optimizations)

## Troubleshooting

### Build Errors
If you encounter Xcode version mismatch errors, ensure the `Directory.Build.targets` file is present with the `_IsMatchingXcode` property set to `true`.

### Connection Failures
- Verify server is running and accessible
- Check firewall rules
- Ensure correct port numbers
- Verify credentials

## Future Enhancements

- [ ] Syntax highlighting in query editor
- [ ] Multiple query tabs
- [ ] Query history
- [ ] Export results to CSV/Excel
- [ ] Stored procedure execution
- [ ] Database schema visualization
- [ ] Dark mode support
- [ ] Connection persistence

## License

MIT License - Feel free to use and modify as needed.

## Author

Built with ❤️ using .NET MAUI on Mac M4
