using FluentValidation;
using HomeFinancial.OfxParser;

namespace HomeFinancial.Application.UseCases.ImportOfxFile;

/// <summary>
/// FluentValidation-валидатор для OFX-транзакции
/// </summary>
public class OfxTransactionValidator : AbstractValidator<TransactionDto>
{
    public OfxTransactionValidator()
    {
        RuleFor(t => t.TranId)
            .NotEmpty().WithMessage(t => $"отсутствует идентификатор транзакции");            
        RuleFor(t => t.Amount)
            .NotNull().WithMessage(t => $"отсутствует сумма");
        RuleFor(t => t.Amount)
            .NotEqual(0).WithMessage(t => $"нулевая сумма");
        RuleFor(t => t.Description)
            .NotEmpty().WithMessage(t => $"отсутствует описание");
        RuleFor(t => t.Category)
            .NotEmpty().WithMessage(t => $"отсутствует категория");
    }
}