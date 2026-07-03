namespace CarteiraInvestimentos.DTOs;

/*
 * É triste ser cabaço no inglês
 */
public record AssetSummaryDto(
    string Ticker,
    int CurrentQuantity, // PrecoMedioCompra
    decimal CurrentMarketPrice, // PrecoAtualMercado
    decimal TotalInvestedValue, // ValorTotalInvestido
    decimal TotalCurrentValue, // ValorTotal
    decimal ReturnPercentage, // RendimentoPercentual
    decimal ProfitOrLoss // LucroNoAtivo
    );