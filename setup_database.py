#!/usr/bin/env python3
"""
Script para configurar o banco de dados do DynamicReportEngine
"""

import sys

try:
    import pymssql
except ImportError:
    print("❌ Biblioteca pymssql não encontrada")
    print("\n📦 Instale com: pip3 install pymssql")
    print("   ou: python3 -m pip install pymssql")
    sys.exit(1)

# Configuração do banco
SERVER = "151.242.149.17"
PORT = 1433
DATABASE = "ReportDB"
USER = "helpdev"
PASSWORD = "8585Gta@85"

def execute_sql(cursor, sql, description):
    """Executa um comando SQL e exibe o resultado"""
    print(f"\n🔧 {description}...")
    try:
        cursor.execute(sql)
        print(f"   ✅ {description} - Concluído")
        return True
    except Exception as e:
        print(f"   ⚠️  {description} - {str(e)}")
        return False

def main():
    print("=" * 60)
    print("  Setup Database - DynamicReportEngine")
    print("=" * 60)
    print(f"\n📡 Conectando ao SQL Server...")
    print(f"   Server: {SERVER}:{PORT}")
    print(f"   Database: {DATABASE}")
    
    try:
        # Conectar ao banco
        conn = pymssql.connect(
            server=SERVER,
            port=PORT,
            user=USER,
            password=PASSWORD,
            database=DATABASE
        )
        cursor = conn.cursor()
        print("   ✅ Conectado com sucesso!\n")
        
        # 1. Limpar tabelas existentes
        print("\n" + "=" * 60)
        print("[1/5] Limpando tabelas existentes...")
        print("=" * 60)
        
        execute_sql(cursor, "IF OBJECT_ID('ReportData', 'U') IS NOT NULL DROP TABLE ReportData", 
                   "Removendo tabela ReportData")
        execute_sql(cursor, "IF OBJECT_ID('ReportField', 'U') IS NOT NULL DROP TABLE ReportField", 
                   "Removendo tabela ReportField")
        execute_sql(cursor, "IF OBJECT_ID('ReportTemplate', 'U') IS NOT NULL DROP TABLE ReportTemplate", 
                   "Removendo tabela ReportTemplate")
        conn.commit()
        
        # 2. Criar tabelas
        print("\n" + "=" * 60)
        print("[2/5] Criando tabelas...")
        print("=" * 60)
        
        # ReportTemplate
        sql_template = """
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
        )
        """
        execute_sql(cursor, sql_template, "Criando tabela ReportTemplate")
        
        # ReportField
        sql_field = """
        CREATE TABLE ReportField (
            Id INT IDENTITY PRIMARY KEY,
            ReportTemplateId INT NOT NULL FOREIGN KEY REFERENCES ReportTemplate(Id) ON DELETE CASCADE,
            FieldName NVARCHAR(100) NOT NULL,
            Label NVARCHAR(100) NULL,
            Section NVARCHAR(50) DEFAULT 'Body',
            FieldType NVARCHAR(50) DEFAULT 'Texto',
            DisplayOrder INT DEFAULT 0,
            Required BIT DEFAULT 0
        )
        """
        execute_sql(cursor, sql_field, "Criando tabela ReportField")
        
        # ReportData
        sql_data = """
        CREATE TABLE ReportData (
            Id INT IDENTITY PRIMARY KEY,
            FieldName NVARCHAR(100) NOT NULL,
            Value NVARCHAR(MAX) NULL,
            CreatedAt DATETIME DEFAULT GETDATE()
        )
        """
        execute_sql(cursor, sql_data, "Criando tabela ReportData")
        
        # Índices
        execute_sql(cursor, "CREATE INDEX IX_ReportField_TemplateId ON ReportField(ReportTemplateId)", 
                   "Criando índice IX_ReportField_TemplateId")
        execute_sql(cursor, "CREATE INDEX IX_ReportData_FieldName ON ReportData(FieldName)", 
                   "Criando índice IX_ReportData_FieldName")
        
        conn.commit()
        
        # 3. Inserir template
        print("\n" + "=" * 60)
        print("[3/5] Inserindo template de relatório...")
        print("=" * 60)
        
        sql_insert_template = """
        INSERT INTO ReportTemplate (Name, Css, HeaderHtml, BodyHtml, FooterHtml)
        VALUES (
            'RelatorioVendas',
            'body { font-family: Arial, sans-serif; font-size: 12px; margin: 20px; color: #333; } 
             h1 { color: #FF6600; margin-bottom: 5px; } 
             .header { text-align: center; border-bottom: 2px solid #FF6600; padding-bottom: 10px; margin-bottom: 20px; }
             .info { background: #f9f9f9; padding: 10px; border-left: 4px solid #FF6600; margin-bottom: 15px; }
             .table { width: 100%%; border-collapse: collapse; margin-top: 15px; }
             .table th { background: #FF6600; color: white; padding: 8px; text-align: left; }
             .table td { padding: 8px; border-bottom: 1px solid #ddd; }
             .footer { text-align: center; margin-top: 30px; font-size: 10px; color: #666; border-top: 1px solid #ccc; padding-top: 10px; }',
            '<div class="header"><h1>Relatório de Vendas - {{Periodo}}</h1><p>{{Empresa}}</p></div>',
            '<div class="info"><p><strong>Vendedor:</strong> {{Vendedor}}</p><p><strong>Região:</strong> {{Regiao}}</p><p><strong>Total:</strong> {{TotalVendas}}</p></div>
             <table class="table"><thead><tr><th>Produto</th><th>Quantidade</th><th>Valor</th></tr></thead><tbody><tr><td>{{Produto}}</td><td>{{Quantidade}}</td><td>{{Valor}}</td></tr></tbody></table>',
            '<div class="footer"><p>Gerado em {{DataGeracao}} por DynamicReportEngine</p></div>'
        )
        """
        execute_sql(cursor, sql_insert_template, "Inserindo template RelatorioVendas")
        conn.commit()
        
        # 4. Inserir campos
        print("\n" + "=" * 60)
        print("[4/5] Inserindo campos do template...")
        print("=" * 60)
        
        fields = [
            ('Periodo', 'Período', 'Header', 'Texto', 1, 1),
            ('Empresa', 'Nome da Empresa', 'Header', 'Texto', 2, 1),
            ('Vendedor', 'Nome do Vendedor', 'Body', 'Texto', 3, 1),
            ('Regiao', 'Região de Vendas', 'Body', 'Texto', 4, 0),
            ('TotalVendas', 'Total de Vendas (R$)', 'Body', 'Moeda', 5, 1),
            ('Produto', 'Nome do Produto', 'Body', 'Texto', 6, 0),
            ('Quantidade', 'Quantidade Vendida', 'Body', 'Numero', 7, 0),
            ('Valor', 'Valor Unitário', 'Body', 'Moeda', 8, 0),
            ('DataGeracao', 'Data de Geração', 'Footer', 'Data', 9, 1)
        ]
        
        for field in fields:
            sql = f"""
            INSERT INTO ReportField (ReportTemplateId, FieldName, Label, Section, FieldType, DisplayOrder, Required)
            SELECT Id, '{field[0]}', '{field[1]}', '{field[2]}', '{field[3]}', {field[4]}, {field[5]}
            FROM ReportTemplate WHERE Name = 'RelatorioVendas'
            """
            execute_sql(cursor, sql, f"Inserindo campo {field[0]}")
        
        conn.commit()
        
        # 5. Inserir dados de exemplo
        print("\n" + "=" * 60)
        print("[5/5] Inserindo dados de exemplo...")
        print("=" * 60)
        
        data = [
            ('Periodo', 'Janeiro/2024'),
            ('Empresa', 'TechCorp Ltda'),
            ('Vendedor', 'João Silva'),
            ('Regiao', 'Sudeste'),
            ('TotalVendas', 'R$ 45.320,00'),
            ('Produto', 'Notebook Dell Inspiron 15'),
            ('Quantidade', '12'),
            ('Valor', 'R$ 3.200,00'),
            ('DataGeracao', '2024-01-31 14:30:00')
        ]
        
        for item in data:
            sql = f"INSERT INTO ReportData (FieldName, Value) VALUES ('{item[0]}', '{item[1]}')"
            execute_sql(cursor, sql, f"Inserindo dado {item[0]}")
        
        conn.commit()
        
        # Verificação
        print("\n" + "=" * 60)
        print("Verificando dados inseridos...")
        print("=" * 60)
        
        cursor.execute("SELECT COUNT(*) FROM ReportTemplate")
        count_templates = cursor.fetchone()[0]
        print(f"   ✅ Templates: {count_templates}")
        
        cursor.execute("SELECT COUNT(*) FROM ReportField")
        count_fields = cursor.fetchone()[0]
        print(f"   ✅ Campos: {count_fields}")
        
        cursor.execute("SELECT COUNT(*) FROM ReportData")
        count_data = cursor.fetchone()[0]
        print(f"   ✅ Dados: {count_data}")
        
        cursor.close()
        conn.close()
        
        print("\n" + "=" * 60)
        print("✅ Setup concluído com sucesso!")
        print("=" * 60)
        print("\n💡 Próximo passo:")
        print("   Execute: dotnet run")
        print()
        
    except Exception as e:
        print(f"\n❌ Erro: {str(e)}")
        sys.exit(1)

if __name__ == "__main__":
    main()

