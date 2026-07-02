using CarteiraInvestimentos.Domain.Entities.Enums;

namespace CarteiraInvestimentos.DTOs;

public record CreateTransactionDto(
    string Ticker,
    int Quantity,
    decimal UnitPrice,
    TransactionType TransactionType
    );