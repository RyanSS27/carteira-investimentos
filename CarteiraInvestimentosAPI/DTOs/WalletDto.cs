namespace CarteiraInvestimentos.DTOs;

public record WalletDto(
    string UserName,
    decimal TotalCurrencyAmount, // patrimônio
    decimal TotalProfitOrLoss,
    DateTime CalculationDate,
    List<AssetSummaryDto> Assets
    );