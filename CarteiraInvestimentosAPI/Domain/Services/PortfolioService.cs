using System.Data.Common;
using CarteiraInvestimentos.Adapters.Infrastructure.Database;
using CarteiraInvestimentos.Domain.Entities;
using CarteiraInvestimentos.Domain.Entities.Enums;
using CarteiraInvestimentos.DTOs;
using CarteiraInvestimentos.Ports;
using Microsoft.EntityFrameworkCore;

namespace CarteiraInvestimentos.Domain.Services;

public class PortfolioService : IPortfolioService
{
    private readonly ApplicationDbContext _context;

    // 1. Injeção de Dependência pelo Construtor (O .NET resolve sozinho)
    public PortfolioService(ApplicationDbContext context)
    {
        _context = context;
    }

    // Usamos o padrão async/await para operações de banco de dados no .NET
    public async Task AddTransactionAsync(Transaction transaction)
    {
        // Correção 1: Acessando a propriedade do Enum diretamente
        if (transaction.TransactionType == TransactionType.BUY)
        {
            // Correção 2: Adiciona na tabela gerenciada pelo DbContext
            _context.Transactions.Add(transaction);
            
            // Correção 3: Salva fisicamente no PostgreSQL
            await _context.SaveChangesAsync();
            return; 
        }
        
        // --- SE FOR UMA VENDA ---
        
        // Usamos o LINQ (Sintaxe expressiva do .NET) para somar as compras e vendas direto no banco
        int totalBought = await _context.Transactions
            .Where(t => t.Ticker == transaction.Ticker && t.TransactionType == TransactionType.BUY)
            .SumAsync(t => t.Quantity);

        int totalSold = await _context.Transactions
            .Where(t => t.Ticker == transaction.Ticker && t.TransactionType == TransactionType.SELL)
            .SumAsync(t => t.Quantity);

        int currentBalance = totalBought - totalSold;

        // Validação de Negócio: Impede que o usuário fique com ações negativas 
        if (transaction.Quantity > currentBalance)
        {
            throw new InvalidOperationException(
                $"Insufficient stock balance to sell {transaction.Ticker}. You currently have {currentBalance} shares.");
        }

        // Se passou na validação, permite o registro da venda
        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync();
    }

    public PortifolioDto GetPortifolioSummary()
    {
        throw new NotImplementedException();
    }
}