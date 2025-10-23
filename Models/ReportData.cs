namespace DynamicReportEngine.Models;

public class ReportData
{
    public int Id { get; set; }
    public string FieldName { get; set; } = string.Empty;
    public string? Value { get; set; }
    public DateTime CreatedAt { get; set; }
}

