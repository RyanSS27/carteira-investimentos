# 📈 Carteira de Investimentos - Web API

Web API desenvolvida na plataforma .NET 8 com persistência em banco de dados relacional PostgreSQL. O objetivo principal do software é a consolidação automatizada de uma carteira de investimentos em ativos de renda variável listados na bolsa de valores brasileira (B3).

## 🚀 Funções Principais do Sistema

* **Registrar Movimentações:** Permitir a entrada e persistência de operações históricas de Compra e Venda de ações.
* **Consolidar Posição:** Calcular em tempo de execução a quantidade atual líquida e o preço médio ponderado de custo de cada ativo.
* **Integrar com Mercado Financeiro:** Realizar chamadas HTTP assíncronas para a API externa Brapi para obter cotações atualizadas.
* **Calcular Rentabilidade:** Computar o ganho ou perda patrimonial, tanto em valores nominais quanto percentuais.

---

## 🏗️ Estrutura do Projeto (Arquitetura Hexagonal Simplificada)

A estrutura abaixo organiza a aplicação separando as responsabilidades, isolando o núcleo de negócios das ferramentas de tecnologia.

```text
📁 CarteiraInvestimentosApp
│
├── 📁 Domain/                      <-- O Coração: Regras puras de negócio (Sem pacotes externos)
│   ├── 📁 Entities/
│   │   └── 📄 Transacao.cs          <-- Entidade C# pura que representa a tabela
│   └── 📁 Services/
│       └── 📄 CarteiraService.cs   <-- Onde vivem as fórmulas matemáticas de cálculo
│
├── 📁 Ports/                       <-- As Fronteiras: Apenas Interfaces (Contratos de código)
│   ├── 📄 ICarteiraService.cs      <-- Interface que expõe as regras de negócio
│   └── 📄 IMercadoFinanceiroService.cs <-- Interface que isola a busca de cotações
│
├── 📁 Adapters/                    <-- Os Músculos: Onde a tecnologia real é implementada
│   │
│   ├── 📁 Driving/                 <-- Entrada: Quem aciona a sua aplicação de fora
│   │   └── 📁 Controllers/
│   │       ├── 📄 TransacoesController.cs <-- Recebe o POST para salvar ordens
│   │       └── 📄 CarteiraController.cs   <-- Recebe o GET para listar o resumo
│   │
│   └── 📁 Infrastructure/          <-- Saída: Ferramentas externas que a aplicação usa
│       ├── 📁 Database/            <-- Persistência de Dados (Entity Framework + PostgreSQL)
│       │   └── 📄 ApplicationDbContext.cs
│       └── 📁 External/            <-- Integrações de Rede (Flurl + API Brapi)
│           └── 📄 MercadoFinanceiroService.cs
│
├── 📁 Dtos/                        <-- Data Transfer Objects: Moldes para entrada/saída de JSON
│   ├── 📄 ResumoCarteiraDto.cs     <-- Estrutura de retorno do GET da carteira
│   └── 📄 AtivoResumoDto.cs        <-- Detalhe de cada ativo dentro do resumo
│
├── 📄 Program.cs                   <-- O Cérebro: Inicializa tudo e injeta as dependências
└── 📄 appsettings.json             <-- Configurações: String de conexão do banco de dados
```
---

## 🛠️ Tecnologias Chave

* **.NET 8 / 10**
* **Entity Framework Core** (Adaptador ORM para PostgreSQL)
* **Flurl** (Consumo fluente da API Brapi)
* **PostgreSQL** (Banco de dados relacional)
