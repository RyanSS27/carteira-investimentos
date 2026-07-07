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
        var allTransactions = await _context.Transactions
            .AsNoTracking() // Remove o cache de rastreamento do EF, liberando memória RAM
            .OrderBy(t => t.Ticker)
            .ThenBy(t => t.TransactionDate) 
            .Select(t => new 
            {
                t.Ticker,
                t.Quantity,
                t.UnitPrice,
                t.TransactionType
            })
            .ToListAsync();
        
        var groups = allTransactions.GroupBy(t => t.Ticker);
        var assetsList = new List<AssetSummaryDto>();

        foreach (var group in groups)
        {
            string ticker = group.Key;
            
            /*
                Ajuste móvel cronológico do preço médio de aquisição: o cálculo atualiza a média de aquisição
                apenas quando você compra novas ações.
                
                Assim, a venda de ações não influência o valor médio de quanto você pagou por elas (isso vinha
                ocorrendo antes)
            */
            
            int currentQuantity = 0;
            decimal totalInvestedValue = 0;
            decimal averageAcquisitionPrice = 0;
            
            foreach (var t in group)
            {
                if (t.TransactionType == TransactionType.BUY)
                {
                    // Nova compra: Incrementa estoque, soma novo capital e recalcula o preço médio
                    totalInvestedValue += (t.Quantity * t.UnitPrice);
                    currentQuantity += t.Quantity;
                    averageAcquisitionPrice = currentQuantity > 0 ? totalInvestedValue / currentQuantity : 0;
                }
                else if (t.TransactionType == TransactionType.SELL)
                {
                    // Venda: Reduz estoque e abate o valor investido proporcionalmente ao preço médio atual
                    currentQuantity -= t.Quantity;
                    totalInvestedValue = currentQuantity * averageAcquisitionPrice;
                }
            }
            
            if (currentQuantity <= 0) continue;

            decimal currentPrice = averageAcquisitionPrice;
            bool isPriceUpToDate = false;

            /*
                O ideal seria fazer uma consulta única com vários Tickers, pois a API da Brapi consegue retornar
                um array com todas as informações. 
                
                Porém, esta opção não está disponível gratuitamente e por isso tenho de conferir Ticker por Ticker.
             */
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
                    Console.WriteLine($"[API WARNING] Não foi encontrado um correspondente ao ticker {ticker}, recebemos: '{apiPrice}'. O valor de custo será aplicado.");
                    Console.ResetColor();
                }
            }
            catch (Exception ex)
            {
                // Imprime os erros para facilitar o debug
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[API ERROR] Falha ao buscar pelo {ticker}. Limites da plataforma de destino ou incompatibilidades com a estrutura.");
                Console.WriteLine($"Detalhes da Exception: {ex.Message}");
                Console.WriteLine($"Rastreamento do erro: {ex.StackTrace}");
                Console.ResetColor();
            }
            
            decimal totalCurrentValue = currentQuantity * currentPrice;
            decimal profitOrLoss = totalCurrentValue - totalInvestedValue;
            decimal returnPercentage = averageAcquisitionPrice > 0 ? ((currentPrice / averageAcquisitionPrice) - 1) * 100 : 0;

            assetsList.Add(new AssetSummaryDto(
                Ticker: ticker,
                CurrentQuantity: currentQuantity,
                AveragePrice: Math.Round(averageAcquisitionPrice, 2),
                CurrentMarketPrice: Math.Round(currentPrice, 2),
                TotalInvestedValue: Math.Round(totalInvestedValue, 2),
                TotalCurrentValue: Math.Round(totalCurrentValue, 2),
                ProfitOrLoss: Math.Round(profitOrLoss, 2),
                ReturnPercentage: Math.Round(returnPercentage, 2),
                IsPriceUpToDate: isPriceUpToDate
            ));
        }
        
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