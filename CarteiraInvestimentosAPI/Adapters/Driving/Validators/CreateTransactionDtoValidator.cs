using CarteiraInvestimentos.DTOs;
using FluentValidation;

namespace CarteiraInvestimentos.Adapters.Driving.Validators;

public class CreateTransactionDtoValidator : AbstractValidator<CreateTransactionDto>
{
    public CreateTransactionDtoValidator()
    {
        RuleFor(x => x.Ticker)
            .NotEmpty().WithMessage("Valor para \"Ticker\" é necessário.")
            .Matches(@"^[A-Z0-9]{5,6}$").WithMessage("Ticker no formato inválido. Dever ter entre 5 e 6 caracteres alfanuméricos (ex: PETR4).");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("\"Quantity\" precisa ser maior que 0.");

        RuleFor(x => x.UnitPrice)
            .GreaterThan(0).WithMessage("\"Unit price\" deve ser superior a 0.");

        RuleFor(x => x.TransactionType)
            .IsInEnum().WithMessage("\"TransactionType\" inválido. Valores esperados: BUY ou SELL.");
    }
}