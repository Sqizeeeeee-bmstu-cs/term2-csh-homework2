using System.Text;

public class ReportBuilder
{
    private DatabaseManager _db;
    private string _sql = "";
    private string _title = "";
    private string[] _headers = Array.Empty<string>();
    private int[] _widths = Array.Empty<int>();

    public ReportBuilder(DatabaseManager db)
    {
        _db = db;
    }

    public ReportBuilder Query(string sql)
    {
        _sql = sql;
        return this;
    }

    public ReportBuilder Title(string title)
    {
        _title = title;
        return this;
    }

    public ReportBuilder Header(params string[] columns)
    {
        _headers = columns;
        return this;
    }

    public ReportBuilder ColumnWidths(params int[] widths)
    {
        _widths = widths;
        return this;
    }

    public string Build()
    {
        var (columns, rows) = _db.ExecuteQuery(_sql);
        var sb = new StringBuilder();
        
        if (!string.IsNullOrEmpty(_title))
        {
            sb.AppendLine($"=== {_title} ===");
            sb.AppendLine();
        }
        
        string[] displayHeaders = _headers.Length > 0 ? _headers : columns;
        int colCount = displayHeaders.Length;
        
        int[] widths;
        if (_widths.Length >= colCount)
        {
            widths = _widths;
        }
        else
        {
            widths = new int[colCount];
            for (int i = 0; i < colCount; i++)
                widths[i] = 20;
        }
        
        for (int i = 0; i < colCount; i++)
            sb.Append(displayHeaders[i].PadRight(widths[i]));
        sb.AppendLine();
        
        int totalWidth = 0;
        for (int i = 0; i < colCount; i++)
            totalWidth += widths[i];
        sb.AppendLine(new string('-', totalWidth));
        
        foreach (var row in rows)
        {
            for (int i = 0; i < colCount && i < row.Length; i++)
                sb.Append(row[i].PadRight(widths[i]));
            sb.AppendLine();
        }
        
        return sb.ToString();
    }

    public void Print()
    {
        Console.WriteLine(Build());
    }

}

