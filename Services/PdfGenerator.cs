using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Serilog;

namespace DynamicReportEngine.Services;

public class PdfGenerator : IPdfGenerator
{
    private readonly ILogger _logger;
    private readonly string _outputDirectory;

    public PdfGenerator(ILogger logger, string outputDirectory)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _outputDirectory = outputDirectory ?? throw new ArgumentNullException(nameof(outputDirectory));

        // Configurar licença do QuestPDF (Community License)
        QuestPDF.Settings.License = LicenseType.Community;

        // Criar diretório de saída se não existir
        if (!Directory.Exists(_outputDirectory))
        {
            Directory.CreateDirectory(_outputDirectory);
            _logger.Information("Diretório de saída criado: {Directory}", _outputDirectory);
        }
    }

    public Task<string> GenerateAsync(string html, string outputFileName)
    {
        _logger.Information("Iniciando geração de PDF: {FileName}", outputFileName);

        try
        {
            var outputPath = Path.Combine(_outputDirectory, outputFileName);

            // Gerar PDF usando QuestPDF
            Document.Create(container =>
            {
                container.Page(page =>
                {
                    // Configurar página A4 com margens de 20mm
                    page.Size(PageSizes.A4);
                    page.Margin(20, Unit.Millimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(12).FontFamily("Arial"));

                    // Renderizar conteúdo HTML simplificado
                    page.Content().Element(container =>
                    {
                        RenderHtmlContent(container, html);
                    });
                });
            }).GeneratePdf(outputPath);

            _logger.Information("PDF gerado com sucesso: {FilePath}", outputPath);
            return Task.FromResult(outputPath);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Erro ao gerar PDF: {FileName}", outputFileName);
            throw;
        }
    }

    private void RenderHtmlContent(IContainer container, string html)
    {
        // QuestPDF não renderiza HTML diretamente, então vamos extrair o conteúdo
        // Para um projeto real, considere usar bibliotecas como HtmlRenderer.PdfSharp
        // ou converter HTML para elementos QuestPDF
        
        // Por simplicidade, vamos extrair o texto e estrutura básica
        var cleanHtml = System.Net.WebUtility.HtmlDecode(html);
        
        // Remover tags HTML para exibição simples
        var textContent = System.Text.RegularExpressions.Regex.Replace(cleanHtml, "<.*?>", " ");
        textContent = System.Text.RegularExpressions.Regex.Replace(textContent, @"\s+", " ").Trim();

        container.Column(column =>
        {
            column.Spacing(10);
            
            // Para este exemplo, vamos usar uma abordagem mais direta
            // Renderizar HTML como texto rico
            ParseAndRenderHtml(column, html);
        });
    }

    private void ParseAndRenderHtml(ColumnDescriptor column, string html)
    {
        // Extrair título
        var titleMatch = System.Text.RegularExpressions.Regex.Match(html, @"<h1[^>]*>(.*?)</h1>");
        if (titleMatch.Success)
        {
            var title = System.Net.WebUtility.HtmlDecode(titleMatch.Groups[1].Value);
            column.Item().AlignCenter().Text(title).FontSize(20).Bold().FontColor(Colors.Orange.Darken2);
        }

        // Extrair parágrafos da seção info
        var infoMatches = System.Text.RegularExpressions.Regex.Matches(html, @"<p[^>]*><strong>(.*?)</strong>\s*(.*?)</p>");
        foreach (System.Text.RegularExpressions.Match match in infoMatches)
        {
            var label = System.Net.WebUtility.HtmlDecode(match.Groups[1].Value);
            var value = System.Net.WebUtility.HtmlDecode(match.Groups[2].Value);
            
            column.Item().Row(row =>
            {
                row.AutoItem().Text(label).Bold();
                row.RelativeItem().PaddingLeft(5).Text(value);
            });
        }

        // Extrair e renderizar tabela
        var tableMatch = System.Text.RegularExpressions.Regex.Match(html, @"<table[^>]*>(.*?)</table>", System.Text.RegularExpressions.RegexOptions.Singleline);
        if (tableMatch.Success)
        {
            column.Item().PaddingVertical(10).Table(table =>
            {
                // Extrair cabeçalhos
                var headerMatches = System.Text.RegularExpressions.Regex.Matches(tableMatch.Value, @"<th[^>]*>(.*?)</th>");
                var headers = headerMatches.Cast<System.Text.RegularExpressions.Match>()
                    .Select(m => System.Net.WebUtility.HtmlDecode(m.Groups[1].Value))
                    .ToList();

                // Definir colunas
                table.ColumnsDefinition(columns =>
                {
                    foreach (var _ in headers)
                    {
                        columns.RelativeColumn();
                    }
                });

                // Cabeçalho
                table.Header(header =>
                {
                    foreach (var headerText in headers)
                    {
                        header.Cell().Background(Colors.Orange.Darken2).Padding(5)
                            .Text(headerText).FontColor(Colors.White).Bold();
                    }
                });

                // Linhas
                var rowMatches = System.Text.RegularExpressions.Regex.Matches(tableMatch.Value, @"<tr[^>]*>(.*?)</tr>", System.Text.RegularExpressions.RegexOptions.Singleline);
                var isEvenRow = false;
                
                foreach (System.Text.RegularExpressions.Match rowMatch in rowMatches)
                {
                    var cellMatches = System.Text.RegularExpressions.Regex.Matches(rowMatch.Groups[1].Value, @"<td[^>]*>(.*?)</td>");
                    if (cellMatches.Count == 0) continue; // Pular linha de cabeçalho

                    foreach (System.Text.RegularExpressions.Match cellMatch in cellMatches)
                    {
                        var cellValue = System.Net.WebUtility.HtmlDecode(cellMatch.Groups[1].Value);
                        
                        if (isEvenRow)
                        {
                            table.Cell().Padding(5).Background(Colors.Grey.Lighten4).Text(cellValue);
                        }
                        else
                        {
                            table.Cell().Padding(5).Text(cellValue);
                        }
                    }
                    
                    isEvenRow = !isEvenRow;
                }
            });
        }

        // Extrair total
        var totalMatch = System.Text.RegularExpressions.Regex.Match(html, @"<strong>Total:</strong>\s*(.*?)</p>");
        if (totalMatch.Success)
        {
            var total = System.Net.WebUtility.HtmlDecode(totalMatch.Groups[1].Value);
            column.Item().PaddingTop(10).Text($"Total: {total}").FontSize(14).Bold();
        }

        // Extrair footer
        var footerMatch = System.Text.RegularExpressions.Regex.Match(html, @"<div class=""footer"">(.*?)</div>", System.Text.RegularExpressions.RegexOptions.Singleline);
        if (footerMatch.Success)
        {
            var footerParagraphs = System.Text.RegularExpressions.Regex.Matches(footerMatch.Groups[1].Value, @"<p[^>]*>(.*?)</p>");
            
            column.Item().PaddingTop(20).BorderTop(1).BorderColor(Colors.Grey.Lighten2).PaddingTop(10);
            
            foreach (System.Text.RegularExpressions.Match p in footerParagraphs)
            {
                var text = System.Net.WebUtility.HtmlDecode(p.Groups[1].Value);
                column.Item().AlignCenter().Text(text).FontSize(9).FontColor(Colors.Grey.Darken1);
            }
        }
    }
}

