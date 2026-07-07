using CarteiraInvestimentos.Domain.Entities;
using CarteiraInvestimentos.DTOs;
using CarteiraInvestimentos.Ports;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace CarteiraInvestimentos.Adapters.Driving.Controllers;

[ApiController]
[Route("api/transactions")]
public class TransactionsController : ControllerBase
{
    // "readonly" faz com que só possa ser atribuído no contrutor e não possa ser alterado
    private readonly IPortfolioService _portfolioService;
    private readonly IValidator<CreateTransactionDto> _validator;

    public TransactionsController(IPortfolioService portfolioService, IValidator<CreateTransactionDto> validator)
    {
        _portfolioService = portfolioService;
        _validator = validator;
    }

    [HttpPost] // Endpoint das transactions
    public async Task<IActionResult> Create([FromBody] CreateTransactionDto dto)
    {
        var validationResult = await _validator.ValidateAsync(dto);

        // Verifica e retorna os erros que ocorreram em forma de mensagens
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(
                e => new { property = e.PropertyName, message = e.ErrorMessage }
                );
            return BadRequest(errors);
        }

        // Instancia a transaction
        var transaction = new Transaction
        {
            Ticker = dto.Ticker.ToUpper().Trim(),
            Quantity = dto.Quantity,
            UnitPrice = dto.UnitPrice,
            TransactionType = dto.TransactionType
        };
        
        await _portfolioService.AddTransactionAsync(transaction);

        // Retorna um positivo 
        return StatusCode(201, new 
        { 
            message = $"Transação de {transaction.TransactionType} de Ticker {transaction.Ticker} salva com sucesso!",
            transactionId = transaction.TransactionId,
            date = transaction.TransactionDate
        });
    }
    
    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary()
    {
        var summary = await _portfolioService.GetPortfolioSummaryAsync();
        return Ok(summary);
    }
}