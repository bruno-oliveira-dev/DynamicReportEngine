namespace DynamicReportEngine.Services;

public interface IReportEngine
{
    Task<string> GenerateReportAsync(string templateName);
}

