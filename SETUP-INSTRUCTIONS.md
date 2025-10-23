# 🚀 Instruções de Setup - DynamicReportEngine

## ⚠️ Problema Atual

O projeto está com um problema de bloqueio de arquivos no macOS que impede a compilação. Isso é uma questão ambiental que pode ser resolvida reiniciando o Mac ou usando as soluções abaixo.

## 📋 Passo 1: Setup do Banco de Dados

Você tem **3 opções** para executar os scripts SQL:

### Opção 1: Azure Data Studio (Recomendado)

1. Abra o **Azure Data Studio**
2. Conecte ao servidor:
   - Server: `151.242.149.17,1433`
   - Database: `ReportDB`
   - Authentication: SQL Login
   - User: `helpdev`
   - Password: `8585Gta@85`
3. Abra o arquivo `setup-database.sql`
4. Clique em **Run** (ou pressione `F5`)
5. Aguarde a conclusão (você verá mensagens de progresso)

### Opção 2: SQL Server Management Studio (SSMS)

1. Abra o **SSMS**
2. Conecte ao servidor com as credenciais acima
3. File → Open → File... → selecione `setup-database.sql`
4. Execute o script (F5)

### Opção 3: Linha de Comando (sqlcmd)

Se você tiver o `sqlcmd` instalado:

```bash
sqlcmd -S 151.242.149.17,1433 -d ReportDB -U helpdev -P "8585Gta@85" -i setup-database.sql
```

## 🔧 Passo 2: Resolver o Problema de Build

Existem algumas soluções:

### Solução 1: Reiniciar o Mac (Mais Simples)

```bash
sudo reboot
```

Após reiniciar, tente:

```bash
cd /Users/macbookairm1/Desenvolvimento/DynamicReportEngine
dotnet clean
dotnet build
dotnet run
```

### Solução 2: Limpar com Permissões Especiais

Abra o Terminal e execute:

```bash
cd /Users/macbookairm1/Desenvolvimento/DynamicReportEngine

# Parar todos os processos dotnet
pkill -9 dotnet

# Aguardar um pouco
sleep 2

# Remover diretórios com permissões
chmod -R 777 bin obj 2>/dev/null
rm -rf bin obj

# Tentar compilar
dotnet build
```

### Solução 3: Usar Docker (Alternativa)

Se o problema persist

ir, você pode executar em um container Docker.

## ✅ Passo 3: Verificar se Está Funcionando

Após resolver o problema de build:

```bash
dotnet run
```

Você deve ver algo como:

```
╔════════════════════════════════════════╗
║   DynamicReportEngine - Iniciando...  ║
╚════════════════════════════════════════╝

========================================
Iniciando geração do relatório: RelatorioVendas
Buscando template: RelatorioVendas
Template carregado com sucesso: 8 campos encontrados
Dados carregados: 8 registros
Iniciando renderização do HTML para template: RelatorioVendas
HTML renderizado: 2.5KB
Iniciando geração de PDF: RelatorioVendas_20251023_143045.pdf
PDF gerado com sucesso: ./Reports/RelatorioVendas_20251023_143045.pdf
========================================
✓ PDF gerado com sucesso: ./Reports/RelatorioVendas_20251023_143045.pdf
✓ Relatório gerado em 1.2s
========================================
```

O PDF estará em: `./Reports/RelatorioVendas_YYYYMMDD_HHMMSS.pdf`

## 🐛 Troubleshooting

### Erro: "Unable to copy file ... Access denied"

Este é o problema de bloqueio de arquivos. Soluções:

1. **Mais simples**: Reinicie o Mac
2. **Alternativa**: Execute os comandos da Solução 2 acima
3. **Se persistir**: 
   ```bash
   # Verificar processos usando os arquivos
   lsof +D /Users/macbookairm1/Desenvolvimento/DynamicReportEngine/bin 2>/dev/null
   
   # Matar processos específicos (substitua PID pelos IDs mostrados acima)
   kill -9 <PID>
   ```

### Erro: "Connection to SQL Server failed"

Verifique:
1. Se você está conectado à internet
2. Se o firewall não está bloqueando a porta 1433
3. Se as credenciais estão corretas no `appsettings.json`

### Erro: "Template não encontrado"

1. Verifique se o script SQL foi executado com sucesso
2. Conecte ao banco e execute:
   ```sql
   SELECT * FROM ReportTemplate WHERE Name = 'RelatorioVendas';
   ```

## 📚 Arquivos Importantes

- `setup-database.sql` - Script completo do banco
- `SQL_SCRIPTS.md` - Documentação dos scripts SQL
- `appsettings.json` - Configuração da aplicação
- `README.md` - Documentação completa do projeto
- `SetupDatabase.cs` - Script C# para setup automático (quando o build funcionar)

## 📞 Próximos Passos

1. ✅ Execute o script SQL no banco de dados
2. ✅ Resolva o problema de build (recomendo reiniciar o Mac)
3. ✅ Execute `dotnet run`
4. ✅ Verifique o PDF gerado em `./Reports/`

## 🎯 Resumo Rápido

```bash
# 1. Execute setup-database.sql no SQL Server (use Azure Data Studio)

# 2. Reinicie o Mac (ou use a Solução 2 acima)

# 3. Após reiniciar:
cd /Users/macbookairm1/Desenvolvimento/DynamicReportEngine
dotnet clean
dotnet build
dotnet run

# 4. Verifique o PDF em ./Reports/
```

---

**Nota**: O problema de build é um issue conhecido no macOS quando há múltiplos processos dotnet ou arquivos bloqueados. A solução mais confiável é reiniciar o sistema.

