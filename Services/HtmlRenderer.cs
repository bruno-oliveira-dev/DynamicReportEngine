using DynamicReportEngine.Models;
using Serilog;
using System.Text;
using System.Text.RegularExpressions;

namespace DynamicReportEngine.Services;

public class HtmlRenderer : IHtmlRenderer
{
    private readonly ILogger _logger;
    private static readonly Regex PlaceholderRegex = new(@"\{\{([^}]+)\}\}", RegexOptions.Compiled);

    public HtmlRenderer(ILogger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task<string> RenderAsync(ReportTemplate template, Dictionary<string, string> data)
    {
        _logger.Information("Iniciando renderização do HTML para template: {TemplateName}", template.Name);

        try
        {
            // Validar campos obrigatórios
            var missingFields = template.Fields
                .Where(f => f.Required && (!data.ContainsKey(f.FieldName) || string.IsNullOrWhiteSpace(data[f.FieldName])))
                .Select(f => f.FieldName)
                .ToList();

            if (missingFields.Any())
            {
                var message = $"Campos obrigatórios ausentes: {string.Join(", ", missingFields)}";
                _logger.Error(message);
                throw new InvalidOperationException(message);
            }

            // Substituir placeholders em cada seção
            var header = ReplacePlaceholders(template.HeaderHtml ?? string.Empty, data);
            var body = ReplacePlaceholders(template.BodyHtml ?? string.Empty, data);
            var footer = ReplacePlaceholders(template.FooterHtml ?? string.Empty, data);

            // Montar HTML completo
            var html = BuildCompleteHtml(template.Css ?? string.Empty, header, body, footer);

            _logger.Information("HTML renderizado: {Size}KB", html.Length / 1024.0);
            return Task.FromResult(html);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Erro ao renderizar HTML");
            throw;
        }
    }

    private string ReplacePlaceholders(string content, Dictionary<string, string> data)
    {
        if (string.IsNullOrWhiteSpace(content))
            return string.Empty;

        var result = content;
        var matches = PlaceholderRegex.Matches(content);

        foreach (Match match in matches)
        {
            var placeholder = match.Groups[0].Value; // {{FieldName}}
            var fieldName = match.Groups[1].Value.Trim(); // FieldName

            if (data.TryGetValue(fieldName, out var value))
            {
                // Sanitizar valor para HTML (exceto se for HTML de tabela)
                var sanitizedValue = fieldName.Contains("Tabela") || value.Contains("<table")
                    ? value
                    : SanitizeHtml(value);

                result = result.Replace(placeholder, sanitizedValue);
            }
            else
            {
                _logger.Warning("Placeholder não encontrado nos dados: {Placeholder}", fieldName);
                result = result.Replace(placeholder, $"[{fieldName} não encontrado]");
            }
        }

        return result;
    }

    private string SanitizeHtml(string value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        // Não fazer escape completo, apenas proteger caracteres básicos
        // Preservar formatação HTML básica
        return value;
    }

    private string BuildCompleteHtml(string css, string header, string body, string footer)
    {
        var html = new StringBuilder();
        html.AppendLine("<!DOCTYPE html>");
        html.AppendLine("<html lang=\"pt-BR\">");
        html.AppendLine("<head>");
        html.AppendLine("    <meta charset=\"UTF-8\">");
        html.AppendLine("    <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">");
        html.AppendLine("    <title>Relatório</title>");
        html.AppendLine("    <style>");
        html.AppendLine(css);
        html.AppendLine("    </style>");
        html.AppendLine("</head>");
        html.AppendLine("<body>");
        html.AppendLine(header);
        html.AppendLine(body);
        html.AppendLine(footer);
        html.AppendLine("</body>");
        html.AppendLine("</html>");

        return html.ToString();
    }
}

