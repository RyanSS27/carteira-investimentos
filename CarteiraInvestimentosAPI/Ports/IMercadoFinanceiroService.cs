namespace CarteiraInvestimentos.Ports;

public interface IMercadoFinanceiroService
{
    Task<decimal?> GetStockPriceAsync(string ticker);
}