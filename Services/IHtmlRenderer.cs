using DynamicReportEngine.Models;

namespace DynamicReportEngine.Services;

public interface IHtmlRenderer
{
    Task<string> RenderAsync(ReportTemplate template, Dictionary<string, string> data);
}

