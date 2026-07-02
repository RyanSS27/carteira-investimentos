using CarteiraInvestimentos.DTOs;
using FluentValidation;

namespace CarteiraInvestimentos.Adapters.Driving.Validators;

public class CreateTransactionDtoValidator : AbstractValidator<CreateTransactionDto>
{
    public CreateTransactionDtoValidator()
    {
        RuleFor(x => x.Ticker)
            .NotEmpty().WithMessage("Ticker is required.")
            .Matches(@"^[A-Z0-9]{5,6}$").WithMessage("Invalid Ticker format. Must be 5 to 6 alphanumeric characters (e.g., PETR4).");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be greater than zero.");

        RuleFor(x => x.UnitPrice)
            .GreaterThan(0).WithMessage("Unit price must be greater than zero.");

        RuleFor(x => x.TransactionType)
            .IsInEnum().WithMessage("Invalid transaction type. Must be BUY or SELL.");
    }
}