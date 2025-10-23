using DynamicReportEngine.Models;

namespace DynamicReportEngine.Repositories;

public interface IReportRepository
{
    Task<ReportTemplate?> GetTemplateByNameAsync(string name);
    Task<List<ReportData>> GetReportDataAsync();
    Task<string?> GetFieldValueAsync(string fieldName);
}

