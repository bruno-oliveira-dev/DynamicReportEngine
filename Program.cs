using DynamicReportEngine.Extensions;
using DynamicReportEngine.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace DynamicReportEngine;

class Program
{
    static async Task<int> Main(string[] args)
    {
        ILogger? logger = null;

        try
        {
            // Carregar configuração
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            // Configurar DI
            var services = new ServiceCollection();
            services.AddSingleton<IConfiguration>(configuration);
            services.AddReportEngineServices(configuration);

            var serviceProvider = services.BuildServiceProvider();
            logger = serviceProvider.GetRequiredService<ILogger>();

            logger.Information("========================================");
            logger.Information("  DynamicReportEngine - Iniciando");
            logger.Information("========================================");
            logger.Information("");

            // Obter o ReportEngine
            var reportEngine = serviceProvider.GetRequiredService<IReportEngine>();

            // Obter o nome do template (padrão ou via argumento)
            string templateName = args.Length > 0 ? args[0] : "AtestadoCapacidadeTecnica";
            
            logger.Information("Gerando relatório: {TemplateName}", templateName);
            logger.Information("");

            // Gerar o relatório
            var pdfPath = await reportEngine.GenerateReportAsync(templateName);

            logger.Information("");
            logger.Information("========================================");
            logger.Information("✓ Relatório gerado com sucesso!");
            logger.Information("========================================");
            logger.Information("Arquivo: {PdfPath}", pdfPath);
            logger.Information("");

            return 0;
        }
        catch (Exception ex)
        {
            if (logger != null)
            {
                logger.Error(ex, "Erro fatal ao executar o DynamicReportEngine");
            }
            else
            {
                Console.WriteLine($"Erro: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }

            return 1;
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
}
