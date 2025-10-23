using DynamicReportEngine.Repositories;
using DynamicReportEngine.Services;
using DynamicReportEngine.Validators;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace DynamicReportEngine.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddReportEngineServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Configurar Serilog
        var logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
            .WriteTo.File(
                configuration["Logging:FilePath"] ?? "./logs/report-engine-.log",
                rollingInterval: RollingInterval.Day,
                outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] {Message:lj}{NewLine}{Exception}")
            .CreateLogger();

        services.AddSingleton<ILogger>(logger);

        // Configurar Repository
        var connectionString = configuration.GetConnectionString("SqlServer");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Connection string 'SqlServer' não encontrada no appsettings.json");
        }

        services.AddSingleton<IReportRepository>(sp =>
            new SqlReportRepository(connectionString, sp.GetRequiredService<ILogger>()));

        // Configurar Services
        services.AddSingleton<IHtmlRenderer, HtmlRenderer>();

        var outputDirectory = configuration["ReportSettings:OutputDirectory"] ?? "./Reports";
        services.AddSingleton<IPdfGenerator>(sp =>
            new PdfGenerator(sp.GetRequiredService<ILogger>(), outputDirectory));

        services.AddSingleton<IReportEngine, ReportEngine>();

        // Configurar Validators
        services.AddSingleton<TemplateValidator>();

        return services;
    }
}

