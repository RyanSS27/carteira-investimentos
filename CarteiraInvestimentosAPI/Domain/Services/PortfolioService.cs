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
    private readonly IMercadoFinanceiroService _mercadoService;

    public PortfolioService(ApplicationDbContext context, IMercadoFinanceiroService mercadoService)
    {
        _context = context;
        _mercadoService = mercadoService;
    }

    // O padrão async/await é usado para operações de banco de dados no .NET
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

    public async Task<PortfolioSummaryDto> GetPortfolioSummaryAsync()
    {
        // Devo otimizar essa busca para não carregar tantos dados na RAM e agrupar tudo depois
        // Acredito que dê para fazer uma query otimizada
        var allTransactions = await _context.Transactions.ToListAsync();
        
        var groups = allTransactions.GroupBy(t => t.Ticker);
        var assetsList = new List<AssetSummaryDto>();

        foreach (var group in groups)
        {
            string ticker = group.Key;

            // Cálculos básicos de quantidade líquida
            int totalBought = group.Where(t => t.TransactionType == TransactionType.BUY).Sum(t => t.Quantity);
            int totalSold = group.Where(t => t.TransactionType == TransactionType.SELL).Sum(t => t.Quantity);
            int currentQuantity = totalBought - totalSold;
            
            if (currentQuantity <= 0) continue;

            /*
                Cálcula do preço médio ponderado: custo para comprar todas as ações / qtde comprada
                Para criar uma estimativa de quanto vale cada ação
            */ 
            decimal totalBoughtCost = group.Where(t => t.TransactionType == TransactionType.BUY).Sum(t => t.Quantity * t.UnitPrice);
            decimal averagePrice = totalBought > 0 ? totalBoughtCost / totalBought : 0;

            decimal currentPrice = averagePrice; // Valor inicial padrão (Fallback)
            bool isPriceUpToDate = false;

            // Consulta à API Externa (Um por Um) com Rastreamento de Erro no Console
            try
            {
                // Faz a chamada individual para o ativo corrente
                decimal? apiPrice = await _mercadoService.GetStockPriceAsync(ticker);

                if (apiPrice.HasValue && apiPrice.Value > 0)
                {
                    currentPrice = apiPrice.Value;
                    isPriceUpToDate = true;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"[API WARNING] Expected valid price for {ticker}, but received: '{apiPrice}'. Applying fallback.");
                    Console.ResetColor();
                }
            }
            catch (Exception ex)
            {
                // Imprime o erro crônicamente no console do servidor para você debugar
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[API ERROR] Failed to fetch or parse price for ticker {ticker}. Destination Platform limits or structure mismatch.");
                Console.WriteLine($"Exception Details: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                Console.ResetColor();
            }

            // Cálculos financeiros do ativo individual
            decimal totalInvestedValue = currentQuantity * averagePrice;
            decimal totalCurrentValue = currentQuantity * currentPrice;
            decimal profitOrLoss = totalCurrentValue - totalInvestedValue;
            decimal returnPercentage = averagePrice > 0 ? ((currentPrice / averagePrice) - 1) * 100 : 0;

            assetsList.Add(new AssetSummaryDto(
                Ticker: ticker,
                CurrentQuantity: currentQuantity,
                AveragePrice: Math.Round(averagePrice, 2),
                CurrentMarketPrice: Math.Round(currentPrice, 2),
                TotalInvestedValue: Math.Round(totalInvestedValue, 2),
                TotalCurrentValue: Math.Round(totalCurrentValue, 2),
                ProfitOrLoss: Math.Round(profitOrLoss, 2),
                ReturnPercentage: Math.Round(returnPercentage, 2),
                IsPriceUpToDate: isPriceUpToDate
            ));
        }

        // Consolidação das três caixas de Totais Gerais da Carteira
        decimal totalValueUpToDate = assetsList.Where(a => a.IsPriceUpToDate).Sum(a => a.TotalCurrentValue);
        decimal totalValueEstimated = assetsList.Where(a => !a.IsPriceUpToDate).Sum(a => a.TotalCurrentValue);
        decimal totalValue = totalValueUpToDate + totalValueEstimated;

        return new PortfolioSummaryDto(
            OwnerName: "Ryan Silva", // Nome fixo 
            TotalValue: Math.Round(totalValue, 2),
            TotalValueUpToDate: Math.Round(totalValueUpToDate, 2),
            TotalValueEstimated: Math.Round(totalValueEstimated, 2),
            CalculationDate: DateTime.UtcNow,
            Assets: assetsList
        );
    }
}