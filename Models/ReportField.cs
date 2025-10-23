namespace DynamicReportEngine.Models;

public class ReportField
{
    public int Id { get; set; }
    public int ReportTemplateId { get; set; }
    public string FieldName { get; set; } = string.Empty;
    public string? Label { get; set; }
    public string Section { get; set; } = "Body";
    public string FieldType { get; set; } = "Texto";
    public int DisplayOrder { get; set; }
    public bool Required { get; set; }
}

