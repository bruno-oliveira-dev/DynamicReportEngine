using DynamicReportEngine.Models;
using Microsoft.Data.SqlClient;
using Serilog;

namespace DynamicReportEngine.Repositories;

public class SqlReportRepository : IReportRepository
{
    private readonly string _connectionString;
    private readonly ILogger _logger;

    public SqlReportRepository(string connectionString, ILogger logger)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ReportTemplate?> GetTemplateByNameAsync(string name)
    {
        _logger.Information("Buscando template: {TemplateName}", name);

        try
        {
            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            // Buscar template
            var templateQuery = @"
                SELECT Id, Name, Css, HeaderHtml, BodyHtml, FooterHtml, Active, Version, CreatedAt, UpdatedAt
                FROM ReportTemplate
                WHERE Name = @Name AND Active = 1";

            ReportTemplate? template = null;

            await using (var command = new SqlCommand(templateQuery, connection))
            {
                command.Parameters.AddWithValue("@Name", name);

                await using var reader = await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    template = new ReportTemplate
                    {
                        Id = reader.GetInt32(0),
                        Name = reader.GetString(1),
                        Css = reader.IsDBNull(2) ? null : reader.GetString(2),
                        HeaderHtml = reader.IsDBNull(3) ? null : reader.GetString(3),
                        BodyHtml = reader.IsDBNull(4) ? null : reader.GetString(4),
                        FooterHtml = reader.IsDBNull(5) ? null : reader.GetString(5),
                        Active = reader.GetBoolean(6),
                        Version = reader.GetInt32(7),
                        CreatedAt = reader.GetDateTime(8),
                        UpdatedAt = reader.IsDBNull(9) ? null : reader.GetDateTime(9)
                    };
                }
            }

            if (template == null)
            {
                _logger.Warning("Template não encontrado: {TemplateName}", name);
                return null;
            }

            // Buscar campos do template
            var fieldsQuery = @"
                SELECT Id, ReportTemplateId, FieldName, Label, Section, FieldType, DisplayOrder, Required
                FROM ReportField
                WHERE ReportTemplateId = @TemplateId
                ORDER BY DisplayOrder";

            await using (var command = new SqlCommand(fieldsQuery, connection))
            {
                command.Parameters.AddWithValue("@TemplateId", template.Id);

                await using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    var field = new ReportField
                    {
                        Id = reader.GetInt32(0),
                        ReportTemplateId = reader.GetInt32(1),
                        FieldName = reader.GetString(2),
                        Label = reader.IsDBNull(3) ? null : reader.GetString(3),
                        Section = reader.GetString(4),
                        FieldType = reader.GetString(5),
                        DisplayOrder = reader.GetInt32(6),
                        Required = reader.GetBoolean(7)
                    };
                    template.Fields.Add(field);
                }
            }

            _logger.Information("Template carregado com sucesso: {FieldCount} campos encontrados", template.Fields.Count);
            return template;
        }
        catch (SqlException ex)
        {
            _logger.Error(ex, "Erro ao conectar ao SQL Server ao buscar template: {TemplateName}", name);
            throw;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Erro inesperado ao buscar template: {TemplateName}", name);
            throw;
        }
    }

    public async Task<List<ReportData>> GetReportDataAsync()
    {
        _logger.Information("Buscando dados do relatório");

        try
        {
            var dataList = new List<ReportData>();

            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = @"
                SELECT Id, FieldName, Value, CreatedAt
                FROM ReportData
                ORDER BY FieldName";

            await using var command = new SqlCommand(query, connection);
            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                var data = new ReportData
                {
                    Id = reader.GetInt32(0),
                    FieldName = reader.GetString(1),
                    Value = reader.IsDBNull(2) ? null : reader.GetString(2),
                    CreatedAt = reader.GetDateTime(3)
                };
                dataList.Add(data);
            }

            _logger.Information("Dados carregados: {DataCount} registros", dataList.Count);
            return dataList;
        }
        catch (SqlException ex)
        {
            _logger.Error(ex, "Erro ao conectar ao SQL Server ao buscar dados");
            throw;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Erro inesperado ao buscar dados");
            throw;
        }
    }

    public async Task<string?> GetFieldValueAsync(string fieldName)
    {
        _logger.Debug("Buscando valor do campo: {FieldName}", fieldName);

        try
        {
            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = @"
                SELECT TOP 1 Value
                FROM ReportData
                WHERE FieldName = @FieldName
                ORDER BY CreatedAt DESC";

            await using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@FieldName", fieldName);

            var result = await command.ExecuteScalarAsync();
            return result?.ToString();
        }
        catch (SqlException ex)
        {
            _logger.Error(ex, "Erro ao conectar ao SQL Server ao buscar campo: {FieldName}", fieldName);
            throw;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Erro inesperado ao buscar campo: {FieldName}", fieldName);
            throw;
        }
    }
}

