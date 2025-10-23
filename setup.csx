#!/usr/bin/env dotnet-script

#r "nuget: Microsoft.Data.SqlClient, 5.1.5"

using Microsoft.Data.SqlClient;
using System;
using System.Threading.Tasks;

const string ConnectionString = "Server=151.242.149.17,1433;Database=ReportDB;User Id=helpdev;Password=8585Gta@85;TrustServerCertificate=True;";

Console.WriteLine("========================================");
Console.WriteLine("  Setup Database - DynamicReportEngine");
Console.WriteLine("========================================\n");

try
{
    await using var connection = new SqlConnection(ConnectionString);
    await connection.OpenAsync();
    Console.WriteLine("✅ Conectado ao SQL Server\n");

    // 1. Limpar tabelas
    Console.WriteLine("[1/5] Limpando tabelas existentes...");
    await ExecuteAsync(connection, "IF OBJECT_ID('ReportData', 'U') IS NOT NULL DROP TABLE ReportData");
    await ExecuteAsync(connection, "IF OBJECT_ID('ReportField', 'U') IS NOT NULL DROP TABLE ReportField");
    await ExecuteAsync(connection, "IF OBJECT_ID('ReportTemplate', 'U') IS NOT NULL DROP TABLE ReportTemplate");

    // 2. Criar tabelas
    Console.WriteLine("\n[2/5] Criando tabelas...");
    
    await ExecuteAsync(connection, @"
        CREATE TABLE ReportTemplate (
            Id INT IDENTITY PRIMARY KEY,
            Name NVARCHAR(100) NOT NULL UNIQUE,
            Css NVARCHAR(MAX) NULL,
            HeaderHtml NVARCHAR(MAX) NULL,
            BodyHtml NVARCHAR(MAX) NULL,
            FooterHtml NVARCHAR(MAX) NULL,
            Active BIT NOT NULL DEFAULT 1,
            Version INT NOT NULL DEFAULT 1,
            CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
            UpdatedAt DATETIME NULL
        )");
    
    await ExecuteAsync(connection, @"
        CREATE TABLE ReportField (
            Id INT IDENTITY PRIMARY KEY,
            ReportTemplateId INT NOT NULL FOREIGN KEY REFERENCES ReportTemplate(Id) ON DELETE CASCADE,
            FieldName NVARCHAR(100) NOT NULL,
            Label NVARCHAR(100) NULL,
            Section NVARCHAR(50) DEFAULT 'Body',
            FieldType NVARCHAR(50) DEFAULT 'Texto',
            DisplayOrder INT DEFAULT 0,
            Required BIT DEFAULT 0
        )");
    
    await ExecuteAsync(connection, @"
        CREATE TABLE ReportData (
            Id INT IDENTITY PRIMARY KEY,
            FieldName NVARCHAR(100) NOT NULL,
            Value NVARCHAR(MAX) NULL,
            CreatedAt DATETIME DEFAULT GETDATE()
        )");
    
    await ExecuteAsync(connection, "CREATE INDEX IX_ReportField_TemplateId ON ReportField(ReportTemplateId)");
    await ExecuteAsync(connection, "CREATE INDEX IX_ReportData_FieldName ON ReportData(FieldName)");

    // 3. Inserir template
    Console.WriteLine("\n[3/5] Inserindo template...");
    await ExecuteAsync(connection, @"
        INSERT INTO ReportTemplate (Name, Css, HeaderHtml, BodyHtml, FooterHtml)
        VALUES (
            'RelatorioVendas',
            'body { font-family: Arial, sans-serif; font-size: 12px; margin: 20px; color: #333; } h1 { color: #FF6600; }',
            '<div class=""header""><h1>Relatório de Vendas - {{Periodo}}</h1><p>{{Empresa}}</p></div>',
            '<div class=""info""><p><strong>Vendedor:</strong> {{Vendedor}}</p><p><strong>Total:</strong> {{TotalVendas}}</p></div>',
            '<div class=""footer""><p>Gerado em {{DataGeracao}}</p></div>'
        )");

    // 4. Inserir campos
    Console.WriteLine("\n[4/5] Inserindo campos...");
    string[] fields = {
        "('Periodo', 'Período', 'Header', 'Texto', 1, 1)",
        "('Empresa', 'Nome da Empresa', 'Header', 'Texto', 2, 1)",
        "('Vendedor', 'Nome do Vendedor', 'Body', 'Texto', 3, 1)",
        "('TotalVendas', 'Total de Vendas', 'Body', 'Moeda', 4, 1)",
        "('DataGeracao', 'Data de Geração', 'Footer', 'Data', 5, 1)"
    };

    foreach (var field in fields)
    {
        await ExecuteAsync(connection, $@"
            INSERT INTO ReportField (ReportTemplateId, FieldName, Label, Section, FieldType, DisplayOrder, Required)
            SELECT Id, * FROM (SELECT {field}) AS T(FieldName, Label, Section, FieldType, DisplayOrder, Required)
            CROSS JOIN ReportTemplate WHERE Name = 'RelatorioVendas'");
    }

    // 5. Inserir dados
    Console.WriteLine("\n[5/5] Inserindo dados de exemplo...");
    string[] data = {
        "('Periodo', 'Janeiro/2024')",
        "('Empresa', 'TechCorp Ltda')",
        "('Vendedor', 'João Silva')",
        "('TotalVendas', 'R$ 45.320,00')",
        "('DataGeracao', '2024-01-31')"
    };

    foreach (var item in data)
    {
        await ExecuteAsync(connection, $"INSERT INTO ReportData (FieldName, Value) VALUES {item}");
    }

    // Verificação
    Console.WriteLine("\n========================================");
    Console.WriteLine("✅ Setup concluído com sucesso!");
    Console.WriteLine("========================================\n");
    Console.WriteLine("Execute: dotnet run\n");
}
catch (Exception ex)
{
    Console.WriteLine($"\n❌ Erro: {ex.Message}");
    Environment.Exit(1);
}

async Task ExecuteAsync(SqlConnection conn, string sql)
{
    try
    {
        await using var cmd = new SqlCommand(sql, conn);
        await cmd.ExecuteNonQueryAsync();
        Console.WriteLine("   ✅");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"   ⚠️  {ex.Message}");
    }
}

