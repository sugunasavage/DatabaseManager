using System.Data;
using System.Globalization;
using System.Text;

namespace DatabaseManager.Helpers;

public class DataTableToStringConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not DataTable dataTable || dataTable.Rows.Count == 0)
            return "No results";

        var sb = new StringBuilder();

        // Get column widths
        var columnWidths = new int[dataTable.Columns.Count];
        for (int i = 0; i < dataTable.Columns.Count; i++)
        {
            columnWidths[i] = dataTable.Columns[i].ColumnName.Length;
            foreach (DataRow row in dataTable.Rows)
            {
                var cellValue = row[i]?.ToString() ?? "NULL";
                if (cellValue.Length > columnWidths[i])
                    columnWidths[i] = Math.Min(cellValue.Length, 50); // Cap at 50 chars
            }
        }

        // Header
        for (int i = 0; i < dataTable.Columns.Count; i++)
        {
            sb.Append(dataTable.Columns[i].ColumnName.PadRight(columnWidths[i] + 2));
        }
        sb.AppendLine();

        // Separator
        for (int i = 0; i < dataTable.Columns.Count; i++)
        {
            sb.Append(new string('-', columnWidths[i] + 2));
        }
        sb.AppendLine();

        // Rows (limit to first 1000 rows for performance)
        int rowCount = Math.Min(dataTable.Rows.Count, 1000);
        for (int i = 0; i < rowCount; i++)
        {
            for (int j = 0; j < dataTable.Columns.Count; j++)
            {
                var cellValue = dataTable.Rows[i][j]?.ToString() ?? "NULL";
                if (cellValue.Length > 50)
                    cellValue = cellValue.Substring(0, 47) + "...";
                sb.Append(cellValue.PadRight(columnWidths[j] + 2));
            }
            sb.AppendLine();
        }

        if (dataTable.Rows.Count > 1000)
        {
            sb.AppendLine($"\n... showing first 1000 of {dataTable.Rows.Count} rows");
        }

        return sb.ToString();
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
