using System.Data;

namespace DatabaseManager.Models;

public class QueryResult
{
    public DataTable? Data { get; set; }
    public string? ErrorMessage { get; set; }
    public bool IsSuccess => ErrorMessage == null;
    public int RowsAffected { get; set; }
    public TimeSpan ExecutionTime { get; set; }
}
