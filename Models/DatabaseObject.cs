using System.Collections.ObjectModel;

namespace DatabaseManager.Models;

public class DatabaseObject
{
    public string Name { get; set; } = string.Empty;
    public string Schema { get; set; } = string.Empty;
    public DatabaseObjectType Type { get; set; }
    public ObservableCollection<DatabaseObject> Children { get; set; } = new();
    public bool IsExpanded { get; set; }
    public bool HasChildren => Children.Count > 0;
}

public enum DatabaseObjectType
{
    Server,
    Database,
    Folder,
    Table,
    View,
    StoredProcedure,
    Function,
    Column
}
