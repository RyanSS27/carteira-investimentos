using System.Text.Json.Serialization;
using CarteiraInvestimentos.Ports;

namespace CarteiraInvestimentos.Adapters.Infrastructure.ExternalServices;

public class BrapiService : IMercadoFinanceiroService
{
    private readonly HttpClient _httpClient;

    public BrapiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<decimal?> GetStockPriceAsync(string ticker)
    {
        string url = $"api/v2/stocks/quote?symbols={ticker}";

        try
        {
            var response = await _httpClient.GetFromJsonAsync<BrapiResponse>(url);

            // Tenta cessar a primeira ação do array
            var assetResult = response?.Results?.FirstOrDefault();
            
            // Navega até o objeto "data" interno e extrai o preço de mercado
            if (assetResult?.Data != null)
            {
                return assetResult.Data.RegularMarketPrice;
            }

            return null;
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine($"[HTTP ERROR] Requisição falha para o ticker: {ticker}");
            
            try
            {
                string rawBody = await _httpClient.GetStringAsync(url);
                Console.WriteLine($"[API RAW RESPONSE]: {rawBody}");
            }
            catch
            {
                Console.WriteLine("[API RAW RESPONSE]: Unavailable (Network/Timeout error).");
            }

            Console.ResetColor();
            throw; 
        }
    }
}

public record BrapiResponse(
    [property: JsonPropertyName("results")] List<BrapiResult> Results
);

public record BrapiResult(
    [property: JsonPropertyName("symbol")] string Symbol,
    [property: JsonPropertyName("data")] BrapiData Data // Mapeia o nó "data" do JSON
);

public record BrapiData(
    [property: JsonPropertyName("regularMarketPrice")] decimal RegularMarketPrice // Preço real dentro de "data"
);