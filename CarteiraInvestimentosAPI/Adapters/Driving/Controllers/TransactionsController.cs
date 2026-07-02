using CarteiraInvestimentos.Domain.Entities;
using CarteiraInvestimentos.DTOs;
using CarteiraInvestimentos.Ports;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace CarteiraInvestimentos.Adapters.Driving.Controllers;

[ApiController]
[Route("api/transactions")] // Define a rota base da API em inglês 
public class TransactionsController : ControllerBase
{
    private readonly IPortfolioService _portfolioService;
    private readonly IValidator<CreateTransactionDto> _validator;

    public TransactionsController(IPortfolioService portfolioService, IValidator<CreateTransactionDto> validator)
    {
        _portfolioService = portfolioService;
        _validator = validator;
    }

    [HttpPost] // Endpoint para registrar uma movimentação [cite: 44]
    public async Task<IActionResult> Create([FromBody] CreateTransactionDto dto)
    {
        // 1. Executa a validação isolada de formato
        var validationResult = await _validator.ValidateAsync(dto);

        if (!validationResult.IsValid)
        {
            // Retorna um espelho limpo contendo as propriedades e mensagens de erro
            var errors = validationResult.Errors.Select(e => new { property = e.PropertyName, message = e.ErrorMessage });
            return BadRequest(errors);
        }

        // 2. Transforma o DTO limpo na Entidade de Negócio (Tratando o texto)
        var transaction = new Transaction
        {
            Ticker = dto.Ticker.ToUpper().Trim(),
            Quantity = dto.Quantity,
            UnitPrice = dto.UnitPrice,
            TransactionType = dto.TransactionType
        };

        // 3. Envia para o serviço core processar e salvar no PostgreSQL
        await _portfolioService.AddTransactionAsync(transaction);

        // 4. Retorna o contrato exato estipulado na especificação técnica [cite: 52]
        return StatusCode(201, new 
        { 
            message = $"Transaction of {transaction.TransactionType} for {transaction.Ticker} recorded successfully!",
            transactionId = transaction.TransactionId,
            date = transaction.TransactionDate
        });
    }
}