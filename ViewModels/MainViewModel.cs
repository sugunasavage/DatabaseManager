using DatabaseManager.Models;
using DatabaseManager.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace DatabaseManager.ViewModels;

public class MainViewModel : INotifyPropertyChanged
{
    private ObservableCollection<ConnectionInfo> _savedConnections = new();
    private ConnectionInfo? _selectedConnection;
    private ObservableCollection<DatabaseObject> _databaseObjects = new();
    private string _queryText = "-- Enter your SQL query here\nSELECT 1;";
    private DataTable? _queryResults;
    private string _statusMessage = "Ready";
    private bool _isExecuting;
    private string _executionTime = "";

    public event PropertyChangedEventHandler? PropertyChanged;

    public ObservableCollection<ConnectionInfo> SavedConnections
    {
        get => _savedConnections;
        set => SetProperty(ref _savedConnections, value);
    }

    public ConnectionInfo? SelectedConnection
    {
        get => _selectedConnection;
        set
        {
            if (SetProperty(ref _selectedConnection, value) && value != null)
            {
                LoadDatabaseObjects();
            }
        }
    }

    public ObservableCollection<DatabaseObject> DatabaseObjects
    {
        get => _databaseObjects;
        set => SetProperty(ref _databaseObjects, value);
    }

    public string QueryText
    {
        get => _queryText;
        set => SetProperty(ref _queryText, value);
    }

    public DataTable? QueryResults
    {
        get => _queryResults;
        set => SetProperty(ref _queryResults, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    public bool IsExecuting
    {
        get => _isExecuting;
        set => SetProperty(ref _isExecuting, value);
    }

    public string ExecutionTime
    {
        get => _executionTime;
        set => SetProperty(ref _executionTime, value);
    }

    public ICommand AddConnectionCommand { get; }
    public ICommand TestConnectionCommand { get; }
    public ICommand ExecuteQueryCommand { get; }
    public ICommand LoadChildrenCommand { get; }

    public MainViewModel()
    {
        AddConnectionCommand = new Command(async () => await AddConnection());
        TestConnectionCommand = new Command(async () => await TestConnection());
        ExecuteQueryCommand = new Command(async () => await ExecuteQuery());
        LoadChildrenCommand = new Command<DatabaseObject>(async (item) => await LoadChildren(item));
    }

    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return false;

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private Task AddConnection()
    {
        var newConnection = new ConnectionInfo
        {
            Name = $"New Connection {SavedConnections.Count + 1}",
            DatabaseType = DatabaseType.SqlServer,
            Server = "localhost"
        };

        SavedConnections.Add(newConnection);
        SelectedConnection = newConnection;

        return Task.CompletedTask;
    }

    private async Task TestConnection()
    {
        if (SelectedConnection == null)
        {
            StatusMessage = "Please select a connection";
            return;
        }

        StatusMessage = "Testing connection...";
        var service = DatabaseServiceFactory.GetService(SelectedConnection.DatabaseType);
        var success = await service.TestConnectionAsync(SelectedConnection);

        StatusMessage = success
            ? "Connection successful!"
            : "Connection failed. Please check your settings.";
    }

    private async Task ExecuteQuery()
    {
        if (SelectedConnection == null)
        {
            StatusMessage = "Please select a connection";
            return;
        }

        if (string.IsNullOrWhiteSpace(QueryText))
        {
            StatusMessage = "Please enter a query";
            return;
        }

        IsExecuting = true;
        StatusMessage = "Executing query...";

        try
        {
            var service = DatabaseServiceFactory.GetService(SelectedConnection.DatabaseType);
            var result = await service.ExecuteQueryAsync(SelectedConnection, QueryText);

            if (result.IsSuccess)
            {
                QueryResults = result.Data;
                ExecutionTime = $"Execution time: {result.ExecutionTime.TotalMilliseconds:F2}ms";
                StatusMessage = result.Data != null
                    ? $"Query returned {result.Data.Rows.Count} row(s)"
                    : $"Query executed successfully. {result.RowsAffected} row(s) affected";
            }
            else
            {
                QueryResults = null;
                StatusMessage = $"Error: {result.ErrorMessage}";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
        finally
        {
            IsExecuting = false;
        }
    }

    private async void LoadDatabaseObjects()
    {
        if (SelectedConnection == null) return;

        DatabaseObjects.Clear();
        StatusMessage = "Loading database objects...";

        try
        {
            var service = DatabaseServiceFactory.GetService(SelectedConnection.DatabaseType);
            var databases = await service.GetDatabasesAsync(SelectedConnection);

            foreach (var dbName in databases)
            {
                var dbNode = new DatabaseObject
                {
                    Name = dbName,
                    Type = DatabaseObjectType.Database,
                    Children = new ObservableCollection<DatabaseObject>
                    {
                        new DatabaseObject { Name = "Tables", Type = DatabaseObjectType.Folder },
                        new DatabaseObject { Name = "Views", Type = DatabaseObjectType.Folder },
                        new DatabaseObject { Name = "Stored Procedures", Type = DatabaseObjectType.Folder }
                    }
                };

                DatabaseObjects.Add(dbNode);
            }

            StatusMessage = $"Loaded {databases.Count} database(s)";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading objects: {ex.Message}";
        }
    }

    private async Task LoadChildren(DatabaseObject item)
    {
        if (SelectedConnection == null) return;
        if (item.Children.Count > 0 && item.Type != DatabaseObjectType.Folder) return;

        try
        {
            var service = DatabaseServiceFactory.GetService(SelectedConnection.DatabaseType);

            if (item.Type == DatabaseObjectType.Folder && item.Name == "Tables")
            {
                var parent = FindParentDatabase(item);
                if (parent != null)
                {
                    item.Children.Clear();
                    var tables = await service.GetTablesAsync(SelectedConnection, parent.Name);
                    foreach (var table in tables)
                    {
                        item.Children.Add(table);
                    }
                }
            }
            else if (item.Type == DatabaseObjectType.Folder && item.Name == "Views")
            {
                var parent = FindParentDatabase(item);
                if (parent != null)
                {
                    item.Children.Clear();
                    var views = await service.GetViewsAsync(SelectedConnection, parent.Name);
                    foreach (var view in views)
                    {
                        item.Children.Add(view);
                    }
                }
            }
            else if (item.Type == DatabaseObjectType.Folder && item.Name == "Stored Procedures")
            {
                var parent = FindParentDatabase(item);
                if (parent != null)
                {
                    item.Children.Clear();
                    var procs = await service.GetStoredProceduresAsync(SelectedConnection, parent.Name);
                    foreach (var proc in procs)
                    {
                        item.Children.Add(proc);
                    }
                }
            }
            else if (item.Type == DatabaseObjectType.Table || item.Type == DatabaseObjectType.View)
            {
                var parent = FindParentDatabase(item);
                if (parent != null)
                {
                    item.Children.Clear();
                    var columns = await service.GetColumnsAsync(SelectedConnection, parent.Name, item.Name);
                    foreach (var column in columns)
                    {
                        item.Children.Add(column);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
    }

    private DatabaseObject? FindParentDatabase(DatabaseObject item)
    {
        foreach (var db in DatabaseObjects)
        {
            if (db.Children.Contains(item))
                return db;

            foreach (var folder in db.Children)
            {
                if (folder.Children.Contains(item))
                    return db;
            }
        }
        return null;
    }
}
