# DynamicReportEngine

🚀 **Motor de geração de relatórios dinâmicos em PDF** a partir de templates HTML armazenados em SQL Server.

## 📋 Características

- **.NET 7** Console Application otimizado para macOS
- **QuestPDF** para geração profissional de PDFs
- **SQL Server** como fonte de dados e templates
- **Serilog** para logging detalhado
- **Dependency Injection** nativa do .NET
- **Async/Await** para operações assíncronas
- **SOLID Principles** e clean architecture

## 🏗️ Estrutura do Projeto

```
DynamicReportEngine/
├── Program.cs                      # Entry point com DI
├── appsettings.json               # Configurações
├── Models/                        # Entidades de domínio
│   ├── ReportTemplate.cs
│   ├── ReportField.cs
│   └── ReportData.cs
├── Repositories/                  # Acesso a dados
│   ├── IReportRepository.cs
│   └── SqlReportRepository.cs
├── Services/                      # Lógica de negócio
│   ├── IHtmlRenderer.cs
│   ├── HtmlRenderer.cs
│   ├── IPdfGenerator.cs
│   ├── PdfGenerator.cs
│   ├── IReportEngine.cs
│   └── ReportEngine.cs
├── Extensions/                    # Extensões
│   └── ServiceCollectionExtensions.cs
└── Validators/                    # Validações
    └── TemplateValidator.cs
```

## 🔧 Pré-requisitos

- **.NET 7 SDK** ou superior
- **macOS** (testado em macOS 11+)
- **SQL Server** acessível (local ou remoto)
- **Conexão de rede** para acessar o banco de dados

## 📦 Instalação

### 1. Clone o repositório

```bash
git clone <seu-repositorio>
cd DynamicReportEngine
```

### 2. Restaure os pacotes NuGet

```bash
dotnet restore
```

### 3. Configure o banco de dados

**👉 IMPORTANTE: Leia [`COMO-EXECUTAR.md`](./COMO-EXECUTAR.md) para instruções detalhadas!**

**Método Recomendado** (Azure Data Studio - gratuito):
```bash
# 1. Instalar
brew install --cask azure-data-studio

# 2. Conectar ao servidor e executar setup-database.sql
```

**Método Alternativo** (sqlcmd):
```bash
sqlcmd -S 151.242.149.17,1433 -U helpdev -P '8585Gta@85' -d ReportDB -i setup-database.sql
```

Isso criará:
- Tabelas: `ReportTemplate`, `ReportField`, `ReportData`
- Dados de exemplo para o template "RelatorioVendas"

### 4. Configure o appsettings.json

Crie ou edite o arquivo `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "SqlServer": "Server=SEU_SERVIDOR;Database=ReportDB;User Id=SEU_USUARIO;Password=SUA_SENHA;TrustServerCertificate=True;"
  },
  "ReportSettings": {
    "OutputDirectory": "./Reports",
    "DefaultPageSize": "A4",
    "DefaultOrientation": "Portrait",
    "EnableCache": false
  },
  "Logging": {
    "MinimumLevel": "Information",
    "FilePath": "./logs/report-engine-.log"
  }
}
```

**⚠️ IMPORTANTE:** Altere a connection string com suas credenciais reais.

## 🚀 Execução

### Modo normal

```bash
dotnet run
```

### Modo release

```bash
dotnet run --configuration Release
```

### Build e execução

```bash
dotnet build
dotnet run --no-build
```

## 📊 Saída Esperada

Ao executar, você verá logs detalhados no console:

```
[10:30:15 INF] ╔════════════════════════════════════════╗
[10:30:15 INF] ║   DynamicReportEngine - Iniciando...  ║
[10:30:15 INF] ╚════════════════════════════════════════╝
[10:30:15 INF] ========================================
[10:30:15 INF] Iniciando geração do relatório: RelatorioVendas
[10:30:15 INF] Buscando template: RelatorioVendas
[10:30:16 INF] Template carregado com sucesso: 8 campos encontrados
[10:30:16 INF] Buscando dados do relatório
[10:30:16 INF] Dados carregados: 8 registros
[10:30:16 INF] Iniciando renderização do HTML para template: RelatorioVendas
[10:30:16 INF] HTML renderizado: 2.5KB
[10:30:16 INF] Iniciando geração de PDF: RelatorioVendas_20250123_103016.pdf
[10:30:17 INF] PDF gerado com sucesso: ./Reports/RelatorioVendas_20250123_103016.pdf
[10:30:17 INF] ========================================
[10:30:17 INF] ✓ PDF gerado com sucesso: ./Reports/RelatorioVendas_20250123_103016.pdf
[10:30:17 INF] ✓ Relatório gerado em 1.2s
[10:30:17 INF] ========================================
[10:30:17 INF] ╔════════════════════════════════════════╗
[10:30:17 INF] ║        Execução Concluída!             ║
[10:30:17 INF] ╚════════════════════════════════════════╝
```

O PDF será gerado em: `./Reports/RelatorioVendas_YYYYMMDD_HHMMSS.pdf`

## 🗄️ Estrutura do Banco de Dados

### Tabela: ReportTemplate

Armazena os templates HTML com CSS.

| Coluna | Tipo | Descrição |
|--------|------|-----------|
| Id | INT | Chave primária |
| Name | NVARCHAR(100) | Nome único do template |
| Css | NVARCHAR(MAX) | Estilos CSS |
| HeaderHtml | NVARCHAR(MAX) | HTML do cabeçalho |
| BodyHtml | NVARCHAR(MAX) | HTML do corpo |
| FooterHtml | NVARCHAR(MAX) | HTML do rodapé |
| Active | BIT | Template ativo |
| Version | INT | Versão do template |
| CreatedAt | DATETIME | Data de criação |
| UpdatedAt | DATETIME | Data de atualização |

### Tabela: ReportField

Define os campos dinâmicos do template.

| Coluna | Tipo | Descrição |
|--------|------|-----------|
| Id | INT | Chave primária |
| ReportTemplateId | INT | FK para ReportTemplate |
| FieldName | NVARCHAR(100) | Nome do campo (usado em `{{FieldName}}`) |
| Label | NVARCHAR(100) | Rótulo amigável |
| Section | NVARCHAR(50) | Seção (Header/Body/Footer) |
| FieldType | NVARCHAR(50) | Tipo (Texto/Imagem/Data/Tabela) |
| DisplayOrder | INT | Ordem de exibição |
| Required | BIT | Campo obrigatório |

### Tabela: ReportData

Armazena os valores dos campos.

| Coluna | Tipo | Descrição |
|--------|------|-----------|
| Id | INT | Chave primária |
| FieldName | NVARCHAR(100) | Nome do campo |
| Value | NVARCHAR(MAX) | Valor do campo |
| CreatedAt | DATETIME | Data de criação |

## 🎨 Exemplo de Template

Os templates usam placeholders no formato `{{NomeDoCampo}}`:

```html
<div class="header">
    <h1>{{TituloRelatorio}}</h1>
</div>

<div class="info-section">
    <p><strong>Cliente:</strong> {{Cliente.Nome}}</p>
    <p><strong>CPF:</strong> {{Cliente.CPF}}</p>
</div>

{{TabelaProdutos}}

<div class="footer">
    <p>Gerado em {{DataAtual}}</p>
</div>
```

## 🔍 Funcionalidades

### 1. SqlReportRepository

- Conexão assíncrona ao SQL Server
- Busca de templates com campos relacionados
- Busca de dados para preenchimento
- Tratamento robusto de erros
- Logging detalhado

### 2. HtmlRenderer

- Substituição de placeholders `{{Campo}}`
- Suporte a campos aninhados `{{Cliente.Nome}}`
- Validação de campos obrigatórios
- Sanitização de valores HTML
- Montagem de HTML completo com CSS

### 3. PdfGenerator (QuestPDF)

- Geração de PDF a partir de HTML
- Configuração A4 Portrait
- Margens de 20mm
- Suporte a múltiplas páginas
- Renderização de tabelas e formatação
- Criação automática de diretório de saída

### 4. ReportEngine (Orquestrador)

- Fluxo completo de geração
- Validação de templates
- Busca e transformação de dados
- Geração do arquivo final
- Medição de performance
- Tratamento global de erros

### 5. TemplateValidator

- Validação de estrutura do template
- Verificação de campos obrigatórios
- Validação de placeholders vs campos
- Verificação de URLs de imagens
- Retorno de lista de erros

## 📝 Logs

Os logs são gravados em:
- **Console**: Saída formatada e colorida
- **Arquivo**: `./logs/report-engine-YYYY-MM-DD.log`

Níveis de log:
- **Information**: Operações normais
- **Warning**: Situações anormais não críticas
- **Error**: Erros que impedem operações
- **Fatal**: Erros que impedem a execução

## 🛠️ Desenvolvimento

### Adicionar novo template

1. Insira na tabela `ReportTemplate`
2. Adicione campos em `ReportField`
3. Insira dados em `ReportData`
4. Execute: `await reportEngine.GenerateReportAsync("NomeDoTemplate")`

### Personalizar PDF

Edite `PdfGenerator.cs` para ajustar:
- Tamanho de página
- Orientação
- Margens
- Fontes
- Cores

### Adicionar validações

Edite `TemplateValidator.cs` para adicionar regras customizadas.

## 🧪 Testes

Para testar com seus próprios dados:

1. Crie um novo template no banco
2. Defina os campos correspondentes
3. Insira dados de teste
4. Altere `Program.cs`:

```csharp
var templateName = "SeuTemplate";
var pdfPath = await reportEngine.GenerateReportAsync(templateName);
```

## 🐛 Troubleshooting

### Erro de conexão ao SQL Server

```
Erro ao conectar ao SQL Server
```

**Solução:**
- Verifique a connection string
- Teste conectividade: `telnet SEU_SERVIDOR 1433`
- Verifique firewall e permissões

### Template não encontrado

```
Template 'Nome' não encontrado ou inativo
```

**Solução:**
- Verifique se o template existe no banco
- Confirme que `Active = 1`
- Verifique o nome (case-sensitive)

### Campos obrigatórios ausentes

```
Campos obrigatórios ausentes: Campo1, Campo2
```

**Solução:**
- Insira dados em `ReportData` para os campos faltantes
- Ou marque os campos como não obrigatórios (`Required = 0`)

### Erro ao gerar PDF

```
Erro ao gerar PDF
```

**Solução:**
- Verifique se o diretório `./Reports` tem permissões de escrita
- Confirme que há espaço em disco
- Verifique logs detalhados em `./logs/`

## 📚 Dependências

- **Microsoft.Data.SqlClient** (5.1.5) - Driver SQL Server
- **Microsoft.Extensions.Configuration** (7.0.0) - Gerenciamento de configuração
- **Microsoft.Extensions.DependencyInjection** (7.0.0) - Injeção de dependência
- **QuestPDF** (2023.12.5) - Geração de PDF
- **Serilog** (3.1.1) - Logging
- **Serilog.Sinks.Console** (5.0.1) - Log no console
- **Serilog.Sinks.File** (5.0.0) - Log em arquivo

## 📄 Licença

Este projeto é um exemplo educacional e pode ser usado livremente.

## 👥 Contribuições

Contribuições são bem-vindas! Para contribuir:

1. Fork o projeto
2. Crie uma branch: `git checkout -b feature/NovaFuncionalidade`
3. Commit suas mudanças: `git commit -m 'Adiciona nova funcionalidade'`
4. Push para a branch: `git push origin feature/NovaFuncionalidade`
5. Abra um Pull Request

## 📧 Contato

Para dúvidas ou sugestões, abra uma issue no repositório.

---

**Desenvolvido com ❤️ usando .NET 7 e QuestPDF**

