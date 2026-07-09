# 📈 Carteira de Investimentos - Web API

Web API desenvolvida na plataforma .NET 10 com persistência em banco de dados relacional PostgreSQL. O objetivo principal do software é a consolidação automatizada de uma carteira de investimentos em ativos de renda variável listados na bolsa de valores brasileira (B3).

## 🚀 Funções Principais do Sistema

* **Registrar Movimentações:** Permitir a entrada e persistência de operações históricas de Compra e Venda de ações.
* **Consolidar Posição:** Calcular em tempo de execução a quantidade atual líquida e o preço médio ponderado de custo de cada ativo.
* **Integrar com Mercado Financeiro:** Realizar chamadas HTTP assíncronas para a API externa Brapi para obter cotações atualizadas.
* **Calcular Rentabilidade:** Computar o ganho ou perda patrimonial, tanto em valores nominais quanto percentuais.

---

## 🏗️ Estrutura do Projeto (Arquitetura Hexagonal Simplificada)

A estrutura abaixo organiza a aplicação separando as responsabilidades, isolando o núcleo de negócios das ferramentas de tecnologia.

```text
📁 CarteiraInvestimentosAPI/
├── 📁 Adapters/                 # Camada de adaptação ao mundo externo
│   ├── 📁 Driving/              # Adaptadores de entrada
│   │   ├── 📁 Controllers/      # Endpoints REST (HTTP) da API
│   │   └── 📁 Validators/       # Validações de entrada de dados (FluentValidation)
│   └── 📁 Infrastructure/       # Adaptadores de saída (Ferramentas e Serviços)
│       ├── 📁 Database/         # Persistência de dados e contexto do EF Core (PostgreSQL)
│       └── 📁 ExternalServices/ # Clientes HTTP para APIs externas (Integração Brapi)
├── 📁 Domain/                   # Implementação das regras de negócio (Independente de Frameworks)
│   ├── 📁 Entities/             # Modelos de negócio centrais e Enums
│   └── 📁 Services/             # Serviços core que executam as regras financeiras
├── 📁 DTOs/                     # Data Transfer Objects 
├── 📁 Migrations/               # Histórico de versionamento do Banco de Dados
├── 📁 Ports/                    # Interfaces de contrato (Garantem a inversão de dependência)
├── 📄 appsettings.json          # Arquivo de configuração (Template sem credenciais reais)
└── 📄 Program.cs                # Inicializador do .NET, Pipeline HTTP e Container de DI
```
---

## 🛠️ Tecnologias Chave

* **.NET 10**
* **Entity Framework Core** (Adaptador ORM para PostgreSQL)
* **FluentValidation** (Validação fortemente tipada e desacoplada para dados de entrada)
* **Flurl** (Consumo fluente e assíncrono da API Brapi)
* **PostgreSQL 16.14** (Banco de dados relacional)

---

## ⚙️ Configuração do Ambiente Local

### 1. Clonar e Restaurar Dependências
```bash
git clone [https://github.com/RyanSS27/carteira-investimentos.git](https://github.com/RyanSS27/carteira-investimentos.git)
cd carteira-investimentos-api
dotnet restore
```
### 2. Configurar Credenciais Seguras (User Secrets)
Para não expor senhas no repositório do Git, utilize a ferramenta de segredos do .NET para injetar sua string de conexão local do PostgreSQL de forma segura:
```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=carteira_investimentos_db;Username=postgres;Password=SUA_SENHA_AQUI"
```
### 3. Executar as Migrations (Criação do Banco)
Com o serviço do PostgreSQL ativo em sua máquina, aplique a estrutura das tabelas usando o EF Core:

```Bash
dotnet ef database update
```
### 4. Rodar a Aplicação
```Bash
dotnet run
```


## 🧪 Como Testar as Requisições
Via Postman:
* A coleção com os exemplos de chamadas prontas está disponível na raiz do repositório.

* Abra o Postman.

* Clique em Import.

* Selecione o arquivo postman/collection.json localizado na pasta do projeto.

* Execute as requisições para os endpoints configurados
