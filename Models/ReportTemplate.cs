namespace DynamicReportEngine.Models;

public class ReportTemplate
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Css { get; set; }
    public string? HeaderHtml { get; set; }
    public string? BodyHtml { get; set; }
    public string? FooterHtml { get; set; }
    public bool Active { get; set; }
    public int Version { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<ReportField> Fields { get; set; } = new();
}

