# 🚀 Como Executar o Setup do Banco

## 📌 Situação Atual

Todos os arquivos do projeto estão criados e prontos, mas há problemas ambientais no macOS impedindo a execução automática:
- Arquivos bloqueados no diretório `bin/` do .NET
- Problemas de permissões e SSL com Python
- Erro de DNS com dotnet tool

## ✅ **SOLUÇÃO MAIS SIMPLES** (Recomendada)

### 1. Instale o Azure Data Studio (gratuito e ideal para Mac)

```bash
brew install --cask azure-data-studio
```

### 2. Abra o Azure Data Studio e conecte ao banco

- Clique em "New Connection"
- Server: `151.242.149.17,1433`
- Authentication: SQL Login
- User: `helpdev`
- Password: `8585Gta@85`
- Database: `ReportDB`
- Trust server certificate: ✅ Yes

### 3. Execute o arquivo SQL

- No Azure Data Studio, clique em "File" → "Open File"
- Selecione o arquivo: `setup-database.sql`
- Clique em "Run" ou pressione `F5`
- Aguarde a conclusão (verá mensagens de progresso)

### 4. Teste o projeto

Depois que o banco estiver configurado, volte ao terminal e execute:

```bash
cd /Users/macbookairm1/Desenvolvimento/DynamicReportEngine
dotnet clean
dotnet build
dotnet run
```

---

## 🔧 **ALTERNATIVA 1**: Usando TablePlus (se já tiver instalado)

1. Abra o TablePlus
2. Crie nova conexão SQL Server:
   - Host: `151.242.149.17`
   - Port: `1433`
   - User: `helpdev`
   - Password: `8585Gta@85`
   - Database: `ReportDB`
3. Abra o arquivo `setup-database.sql` e execute

---

## 🔧 **ALTERNATIVA 2**: Instalar sqlcmd

```bash
# Instalar via Homebrew
brew tap microsoft/mssql-release https://github.com/Microsoft/homebrew-mssql-release
brew install msodbcsql17 mssql-tools

# Executar o setup
/usr/local/opt/mssql-tools/bin/sqlcmd -S 151.242.149.17,1433 -U helpdev -P '8585Gta@85' -d ReportDB -i setup-database.sql
```

---

## 🔧 **ALTERNATIVA 3**: Executar comandos SQL manualmente

Se preferir, você pode copiar e colar os comandos SQL diretamente no Azure Data Studio ou qualquer cliente SQL:

1. Conecte ao banco
2. Abra o arquivo `setup-database.sql`
3. Copie todo o conteúdo
4. Cole em uma nova Query e execute

---

## 📂 Arquivos Criados

Estes arquivos estão prontos no projeto:

- ✅ `setup-database.sql` - Script SQL completo (MAIS IMPORTANTE)
- ✅ `setup_database.py` - Script Python (requer pymssql)
- ✅ `setup.csx` - Script C# (requer dotnet-script)  
- ✅ `SetupDatabase.cs` - Programa C# compilável

---

## 🎯 Após Configurar o Banco

Uma vez que o banco estiver configurado, você pode executar o projeto principal:

```bash
cd /Users/macbookairm1/Desenvolvimento/DynamicReportEngine
dotnet run
```

O programa irá:
1. Conectar ao SQL Server
2. Buscar o template "RelatorioVendas"
3. Renderizar o HTML com os dados
4. Gerar o PDF em `./Reports/`

---

## 💡 Dica

O **Azure Data Studio** é a ferramenta oficial da Microsoft para trabalhar com SQL Server no macOS e é totalmente gratuita. É a opção mais confiável para este caso.

```bash
brew install --cask azure-data-studio
```

---

## ❓ Problemas?

Se encontrar algum erro durante a execução do SQL, verifique:
1. A conexão com o servidor SQL está funcionando
2. O usuário `helpdev` tem permissões para criar tabelas
3. O database `ReportDB` existe

Para testar a conexão:

```bash
# Com Azure Data Studio: Use o botão "Test Connection"
# Ou se tiver sqlcmd instalado:
/usr/local/opt/mssql-tools/bin/sqlcmd -S 151.242.149.17,1433 -U helpdev -P '8585Gta@85' -d ReportDB -Q "SELECT @@VERSION"
```

