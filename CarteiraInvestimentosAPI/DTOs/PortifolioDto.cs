namespace CarteiraInvestimentos.DTOs;

public record PortifolioDto(
    string UserName,
    decimal TotalCurrencyAmount, // patrimônio
    decimal TotalProfitOrLoss,
    DateTime CalculationDate,
    List<AssetSummaryDto> Assets
    );