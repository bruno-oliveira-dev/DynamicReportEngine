namespace DynamicReportEngine.Services;

public interface IPdfGenerator
{
    Task<string> GenerateAsync(string html, string outputFileName);
}

