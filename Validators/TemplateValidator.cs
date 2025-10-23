using DynamicReportEngine.Models;
using System.Text.RegularExpressions;

namespace DynamicReportEngine.Validators;

public class TemplateValidator
{
    private static readonly Regex PlaceholderRegex = new(@"\{\{([^}]+)\}\}", RegexOptions.Compiled);
    private static readonly Regex UrlRegex = new(@"^https?://", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public List<string> Validate(ReportTemplate template)
    {
        var errors = new List<string>();

        if (template == null)
        {
            errors.Add("Template não pode ser nulo");
            return errors;
        }

        if (string.IsNullOrWhiteSpace(template.Name))
        {
            errors.Add("Nome do template é obrigatório");
        }

        // Validar se há pelo menos uma seção com conteúdo
        if (string.IsNullOrWhiteSpace(template.HeaderHtml) &&
            string.IsNullOrWhiteSpace(template.BodyHtml) &&
            string.IsNullOrWhiteSpace(template.FooterHtml))
        {
            errors.Add("Template deve ter pelo menos uma seção (Header, Body ou Footer) com conteúdo");
        }

        // Validar campos obrigatórios
        if (!template.Fields.Any())
        {
            errors.Add("Template deve ter pelo menos um campo definido");
        }

        // Extrair todos os placeholders do HTML
        var allHtml = $"{template.HeaderHtml} {template.BodyHtml} {template.FooterHtml}";
        var placeholders = ExtractPlaceholders(allHtml);

        // Validar se todos os placeholders têm campos correspondentes
        var fieldNames = template.Fields.Select(f => f.FieldName).ToHashSet();
        var missingFields = placeholders.Where(p => !fieldNames.Contains(p)).ToList();

        if (missingFields.Any())
        {
            errors.Add($"Placeholders sem campos correspondentes: {string.Join(", ", missingFields)}");
        }

        // Validar campos de imagem (devem ser URLs válidas nos dados)
        var imageFields = template.Fields.Where(f => f.FieldType.Equals("Imagem", StringComparison.OrdinalIgnoreCase));
        foreach (var imageField in imageFields)
        {
            if (!placeholders.Contains(imageField.FieldName))
            {
                errors.Add($"Campo de imagem '{imageField.FieldName}' não está sendo usado no template");
            }
        }

        // Validar campos obrigatórios
        var requiredFields = template.Fields.Where(f => f.Required).ToList();
        if (requiredFields.Any() && !fieldNames.Any())
        {
            errors.Add("Template tem campos obrigatórios mas nenhum campo definido");
        }

        return errors;
    }

    private HashSet<string> ExtractPlaceholders(string html)
    {
        if (string.IsNullOrWhiteSpace(html))
            return new HashSet<string>();

        var placeholders = new HashSet<string>();
        var matches = PlaceholderRegex.Matches(html);

        foreach (Match match in matches)
        {
            var fieldName = match.Groups[1].Value.Trim();
            placeholders.Add(fieldName);
        }

        return placeholders;
    }

    public bool IsValidUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return false;

        return UrlRegex.IsMatch(url);
    }
}

