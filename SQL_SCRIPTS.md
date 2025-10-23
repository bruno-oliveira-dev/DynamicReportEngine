# Scripts SQL - DynamicReportEngine

Execute estes scripts no seu SQL Server para configurar o banco de dados.

## 1. Criar as Tabelas

```sql
-- 1. ReportTemplate
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
);

-- 2. ReportField
CREATE TABLE ReportField (
    Id INT IDENTITY PRIMARY KEY,
    ReportTemplateId INT NOT NULL FOREIGN KEY REFERENCES ReportTemplate(Id) ON DELETE CASCADE,
    FieldName NVARCHAR(100) NOT NULL,
    Label NVARCHAR(100) NULL,
    Section NVARCHAR(50) DEFAULT 'Body',
    FieldType NVARCHAR(50) DEFAULT 'Texto',
    DisplayOrder INT DEFAULT 0,
    Required BIT DEFAULT 0
);

-- 3. ReportData
CREATE TABLE ReportData (
    Id INT IDENTITY PRIMARY KEY,
    FieldName NVARCHAR(100) NOT NULL,
    Value NVARCHAR(MAX) NULL,
    CreatedAt DATETIME DEFAULT GETDATE()
);

-- Índices para performance
CREATE INDEX IX_ReportField_TemplateId ON ReportField(ReportTemplateId);
CREATE INDEX IX_ReportData_FieldName ON ReportData(FieldName);
```

## 2. Inserir Template de Exemplo (Relatório de Vendas)

```sql
-- Template de Vendas
INSERT INTO ReportTemplate (Name, Css, HeaderHtml, BodyHtml, FooterHtml)
VALUES ('RelatorioVendas',
       'body { font-family: Arial, sans-serif; font-size: 12px; margin: 20px; color: #333; } 
        h1 { color: #FF6600; margin-bottom: 5px; } 
        .header { text-align: center; border-bottom: 2px solid #FF6600; padding-bottom: 10px; margin-bottom: 20px; } 
        .footer { text-align: center; border-top: 1px solid #ccc; padding-top: 10px; margin-top: 30px; font-size: 10px; color: #666; } 
        table { width: 100%; border-collapse: collapse; margin-top: 15px; } 
        th { background-color: #FF6600; color: white; padding: 8px; text-align: left; } 
        td { border: 1px solid #ddd; padding: 8px; } 
        tr:nth-child(even) { background-color: #f9f9f9; }
        .info-section { margin: 15px 0; line-height: 1.6; }
        .logo { max-width: 150px; }',
       '<div class="header">
            <img src="{{LogoEmpresa}}" class="logo" alt="Logo"/>
            <h1>{{TituloRelatorio}}</h1>
        </div>',
       '<div class="info-section">
            <p><strong>Cliente:</strong> {{Cliente.Nome}}</p>
            <p><strong>CPF:</strong> {{Cliente.CPF}}</p>
            <p><strong>Email:</strong> {{Cliente.Email}}</p>
        </div>
        {{TabelaProdutos}}
        <div class="info-section">
            <p><strong>Total:</strong> {{Total}}</p>
        </div>',
       '<div class="footer">
            <p>Relatório gerado em {{DataAtual}} | Empresa XYZ Ltda - CNPJ: 00.000.000/0001-00</p>
            <p>Rua Exemplo, 123 - São Paulo/SP - Tel: (11) 1234-5678</p>
        </div>'
);
```

## 3. Inserir Campos do Template

```sql
-- Campos do Relatório
DECLARE @TemplateId INT = (SELECT Id FROM ReportTemplate WHERE Name='RelatorioVendas');

INSERT INTO ReportField (ReportTemplateId, FieldName, Label, Section, FieldType, DisplayOrder, Required)
VALUES
(@TemplateId, 'LogoEmpresa', 'Logo da Empresa', 'Header', 'Imagem', 1, 1),
(@TemplateId, 'TituloRelatorio', 'Título do Relatório', 'Header', 'Texto', 2, 1),
(@TemplateId, 'Cliente.Nome', 'Nome do Cliente', 'Body', 'Texto', 3, 1),
(@TemplateId, 'Cliente.CPF', 'CPF do Cliente', 'Body', 'Texto', 4, 1),
(@TemplateId, 'Cliente.Email', 'Email do Cliente', 'Body', 'Texto', 5, 0),
(@TemplateId, 'TabelaProdutos', 'Produtos Vendidos', 'Body', 'Tabela', 6, 1),
(@TemplateId, 'Total', 'Valor Total', 'Body', 'Texto', 7, 1),
(@TemplateId, 'DataAtual', 'Data de Geração', 'Footer', 'Data', 8, 1);
```

## 4. Inserir Dados de Exemplo

```sql
-- Dados de Exemplo
INSERT INTO ReportData (FieldName, Value) VALUES
('LogoEmpresa', 'https://via.placeholder.com/150x50/FF6600/FFFFFF?text=EMPRESA+XYZ'),
('TituloRelatorio', 'Relatório de Vendas - Janeiro 2025'),
('Cliente.Nome', 'Bruno Oliveira da Silva'),
('Cliente.CPF', '123.456.789-00'),
('Cliente.Email', 'bruno.silva@email.com'),
('TabelaProdutos', '<table><thead><tr><th>Código</th><th>Produto</th><th>Qtd</th><th>Preço Unit.</th><th>Subtotal</th></tr></thead><tbody><tr><td>001</td><td>Notebook Dell Inspiron 15</td><td>1</td><td>R$ 3.500,00</td><td>R$ 3.500,00</td></tr><tr><td>002</td><td>Mouse Logitech MX Master 3</td><td>2</td><td>R$ 450,00</td><td>R$ 900,00</td></tr><tr><td>003</td><td>Teclado Mecânico Keychron K2</td><td>1</td><td>R$ 650,00</td><td>R$ 650,00</td></tr><tr><td>004</td><td>Webcam Logitech C920</td><td>1</td><td>R$ 550,00</td><td>R$ 550,00</td></tr></tbody></table>'),
('Total', 'R$ 5.600,00'),
('DataAtual', CONVERT(VARCHAR, GETDATE(), 103));
```

## 5. Verificar Dados Inseridos

```sql
-- Verificar template
SELECT * FROM ReportTemplate WHERE Name = 'RelatorioVendas';

-- Verificar campos
SELECT rf.*, rt.Name AS TemplateName
FROM ReportField rf
INNER JOIN ReportTemplate rt ON rf.ReportTemplateId = rt.Id
WHERE rt.Name = 'RelatorioVendas'
ORDER BY rf.DisplayOrder;

-- Verificar dados
SELECT * FROM ReportData ORDER BY FieldName;
```

## 6. Limpar Dados (Opcional)

```sql
-- Limpar todos os dados (use com cuidado!)
DELETE FROM ReportData;
DELETE FROM ReportField;
DELETE FROM ReportTemplate;

-- Resetar identities
DBCC CHECKIDENT ('ReportTemplate', RESEED, 0);
DBCC CHECKIDENT ('ReportField', RESEED, 0);
DBCC CHECKIDENT ('ReportData', RESEED, 0);
```

## 7. Consultas Úteis

### Listar todos os templates ativos
```sql
SELECT Id, Name, Version, CreatedAt, UpdatedAt
FROM ReportTemplate
WHERE Active = 1
ORDER BY Name;
```

### Contar campos por template
```sql
SELECT 
    rt.Name,
    COUNT(rf.Id) AS TotalCampos,
    SUM(CASE WHEN rf.Required = 1 THEN 1 ELSE 0 END) AS CamposObrigatorios
FROM ReportTemplate rt
LEFT JOIN ReportField rf ON rt.Id = rf.ReportTemplateId
WHERE rt.Active = 1
GROUP BY rt.Name;
```

### Verificar campos sem dados
```sql
SELECT rf.FieldName, rf.Label, rf.Required
FROM ReportField rf
INNER JOIN ReportTemplate rt ON rf.ReportTemplateId = rt.Id
WHERE rt.Name = 'RelatorioVendas'
  AND rf.FieldName NOT IN (SELECT FieldName FROM ReportData)
ORDER BY rf.DisplayOrder;
```

### Buscar templates que usam um campo específico
```sql
SELECT DISTINCT rt.Name, rt.Version
FROM ReportTemplate rt
INNER JOIN ReportField rf ON rt.Id = rf.ReportTemplateId
WHERE rf.FieldName LIKE '%Cliente%'
ORDER BY rt.Name;
```

## Notas

- Certifique-se de ter permissões adequadas no SQL Server
- Os scripts assumem que o banco de dados já existe
- Ajuste os valores de exemplo conforme necessário
- Use transações para segurança:

```sql
BEGIN TRANSACTION;
-- Seus INSERTs aqui
COMMIT;
-- ou ROLLBACK; em caso de erro
```

