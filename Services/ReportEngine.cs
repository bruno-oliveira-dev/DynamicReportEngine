using DynamicReportEngine.Repositories;
using DynamicReportEngine.Validators;
using Serilog;
using System.Diagnostics;

namespace DynamicReportEngine.Services;

public class ReportEngine : IReportEngine
{
    private readonly IReportRepository _repository;
    private readonly IHtmlRenderer _htmlRenderer;
    private readonly IPdfGenerator _pdfGenerator;
    private readonly TemplateValidator _templateValidator;
    private readonly ILogger _logger;

    public ReportEngine(
        IReportRepository repository,
        IHtmlRenderer htmlRenderer,
        IPdfGenerator pdfGenerator,
        TemplateValidator templateValidator,
        ILogger logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _htmlRenderer = htmlRenderer ?? throw new ArgumentNullException(nameof(htmlRenderer));
        _pdfGenerator = pdfGenerator ?? throw new ArgumentNullException(nameof(pdfGenerator));
        _templateValidator = templateValidator ?? throw new ArgumentNullException(nameof(templateValidator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<string> GenerateReportAsync(string templateName)
    {
        var stopwatch = Stopwatch.StartNew();
        _logger.Information("========================================");
        _logger.Information("Iniciando geração do relatório: {TemplateName}", templateName);

        try
        {
            // 1. Buscar template e campos do banco
            var template = await _repository.GetTemplateByNameAsync(templateName);
            if (template == null)
            {
                throw new InvalidOperationException($"Template '{templateName}' não encontrado ou inativo");
            }

            // 2. Validar template
            var validationErrors = _templateValidator.Validate(template);
            if (validationErrors.Any())
            {
                var errorMessage = string.Join("; ", validationErrors);
                _logger.Error("Erros de validação do template: {Errors}", errorMessage);
                throw new InvalidOperationException($"Template inválido: {errorMessage}");
            }

            // 3. Buscar dados do banco
            var reportData = await _repository.GetReportDataAsync();
            
            // Agrupar por FieldName e pegar o valor mais recente (último registro)
            var dataDict = reportData
                .GroupBy(d => d.FieldName)
                .ToDictionary(
                    g => g.Key, 
                    g => g.OrderByDescending(d => d.CreatedAt).First().Value ?? string.Empty
                );

            _logger.Information("Dados carregados: {DataCount} registros", dataDict.Count);

            // 4. Renderizar HTML completo
            var html = await _htmlRenderer.RenderAsync(template, dataDict);

            // 5. Gerar PDF
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var fileName = $"{templateName}_{timestamp}.pdf";
            var pdfPath = await _pdfGenerator.GenerateAsync(html, fileName);

            stopwatch.Stop();
            _logger.Information("========================================");
            _logger.Information("✓ PDF gerado com sucesso: {FilePath}", pdfPath);
            _logger.Information("✓ Relatório gerado em {ElapsedSeconds:F1}s", stopwatch.Elapsed.TotalSeconds);
            _logger.Information("========================================");

            return pdfPath;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.Error(ex, "✗ Erro ao gerar relatório: {TemplateName} (tempo decorrido: {ElapsedSeconds:F1}s)", 
                templateName, stopwatch.Elapsed.TotalSeconds);
            throw;
        }
    }
}

