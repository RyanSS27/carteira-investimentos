namespace CarteiraInvestimentos.DTOs;

public record PortfolioSummaryDto(
    string OwnerName,
    decimal TotalValue, 
    decimal TotalValueUpToDate, // ativos que conseguir consultar
    decimal TotalValueEstimated, // soma das ações não retornadas pela API da brapi
    DateTime CalculationDate,
    List<AssetSummaryDto> Assets
    );