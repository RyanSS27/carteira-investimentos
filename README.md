# 📈 Carteira de Investimentos - Web API

Web API desenvolvida na plataforma .NET 10 com persistência em banco de dados relacional PostgreSQL. O objetivo principal do software é a consolidação automatizada de uma carteira de investimentos em ativos de renda variável listados na bolsa de valores brasileira (B3).

## 🚀 Funcionalidades

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

## 🛠️ Tecnologias 

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
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=VALOR_PORTA_LOCAL;Database=carteira_investimentos_db;Username=postgres;Password=SUA_SENHA_AQUI"
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
---

## 🧪 Como Utilizar

### Endpoints Principais

#### 1. Registrar Transação
* **Rota:** `POST http://localhost:[PORTA-LOCAL]/api/transactions/`
* **Valores válidos para `transactionType`:** `BUY` (ou `0`), `SELL` (ou `1`).
* **Exemplo de Payload (Body):**
```json
{
  "ticker": "ITUB4",
  "quantity": 100,
  "unitPrice": 27.50,
  "transactionType": "BUY"
}

```

* **Retorno de Sucesso (201):**

```json
{
    "message": "Transação de SELL de Ticker ITUB4 salva com sucesso!",
    "transactionId": "49fcfc08-1773-4169-85a3-d5878883a3fa",
    "date": "2026-07-13T17:00:19.5017595Z"
}

```

#### 2. Consulta de Posição da Carteira

* **Rota:** `GET http://localhost:[PORTA-LOCAL]/api/transactions/summary`
* **Retorno de Sucesso (200):**

```json
{
    "ownerName": "Ryan Silva",
    "totalValue": 942.72,
    "totalValueUpToDate": 162.72,
    "totalValueEstimated": 780.00,
    "calculationDate": "2026-07-13T17:00:30.113622Z",
    "assets": [
        {
            "ticker": "PETR4",
            "currentQuantity": 4,
            "averagePrice": 30.00,
            "currentMarketPrice": 40.68,
            "totalInvestedValue": 120.00,
            "totalCurrentValue": 162.72,
            "returnPercentage": 35.60,
            "profitOrLoss": 42.72,
            "isPriceUpToDate": true
        },
        {
            "ticker": "TESTE4",
            "currentQuantity": 26,
            "averagePrice": 30.00,
            "currentMarketPrice": 30.00,
            "totalInvestedValue": 780.00,
            "totalCurrentValue": 780.00,
            "returnPercentage": 0,
            "profitOrLoss": 0.00,
            "isPriceUpToDate": false
        }
    ]
}

```

### Execução via Postman

1. Abra o Postman e clique em **Import**.
2. Selecione o arquivo `postman-tests/CarteiraInvestimentosAPI.postman_collection.json` localizado na pasta do projeto.
3. ⚠️ **ATENÇÃO:** As requisições vêm configuradas por padrão para o endereço de desenvolvimento local `http://localhost:5195/`. Caso a sua aplicação esteja rodando em outra porta (verifique o terminal ao iniciar o projeto), lembre-se de alterar o domínio/porta nas rotas da coleção do Postman antes de enviar.
4. Execute as chamadas desejadas.

