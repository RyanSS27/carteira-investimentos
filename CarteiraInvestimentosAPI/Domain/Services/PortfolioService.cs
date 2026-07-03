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
    
    public PortfolioService(ApplicationDbContext context)
    {
        _context = context;
    }

    // Usamos o padrão async/await para operações de banco de dados no .NET
    public async Task AddTransactionAsync(Transaction transaction)
    {
        
        if (transaction.TransactionType == TransactionType.BUY)
        {
            _context.Transactions.Add(transaction);
            
            await _context.SaveChangesAsync();
            return; 
        }
        
        
        /*
            Somar as compras e vendas direto no banco
            Pesquisar melhor sobre o LINQ que é traduzido em queries no banco
        */
        int quantityTotalBuy = await _context.Transactions
            .Where(t => t.Ticker == transaction.Ticker && t.TransactionType == TransactionType.BUY)
            .SumAsync(t => t.Quantity);

        int quantityTotalSell = await _context.Transactions
            .Where(t => t.Ticker == transaction.Ticker && t.TransactionType == TransactionType.SELL)
            .SumAsync(t => t.Quantity);

        int currentBalance = quantityTotalBuy - quantityTotalSell; // Total de ações restantes ainda vigentes

        
        if (transaction.Quantity > currentBalance)
        {
            throw new InvalidOperationException(
                $"Não há ações suficientes de {transaction.Ticker} para a venda. Há {currentBalance} ações disponíveis.");
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