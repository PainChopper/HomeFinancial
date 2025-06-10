using FluentValidation;
using HomeFinancial.OfxParser.Dto;

namespace HomeFinancial.Application.UseCases.ImportOfxFile;

/// <summary>
/// Валидатор для OfxTransactionDto.
/// </summary>
public class OfxTransactionValidator : AbstractValidator<OfxTransactionDto>
{
    /// <summary>
    /// Конструктор.
    /// </summary>
    public OfxTransactionValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("FITID (Id транзакции) не должен быть пустым.");

        RuleFor(x => x.TranType)
            .NotEmpty().WithMessage("Тип транзакции (TranType) не должен быть пустым.");

        RuleFor(x => x.TranDate)
            .NotEqual(default(DateTime)).WithMessage("Дата транзакции (TranDate) должна быть указана.");
        
        // Поля Category (MEMO) и Description (NAME) могут быть пустыми в OFX,
        // поэтому явных правил на NotEmpty для них нет, если не требуется бизнес-логикой.

        // Сумма транзакции (Amount) обычно не должна быть нулевой для фактических операций.
        // RuleFor(x => x.Amount)
        //     .NotEqual(0).WithMessage("Сумма транзакции (Amount) не должна быть нулевой.");
        // Раскомментируйте и настройте правило для Amount при необходимости.
    }
}