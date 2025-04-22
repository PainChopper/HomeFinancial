using FluentValidation;
using HomeFinancial.Domain.Entities;

namespace HomeFinancial.Application.Validators;

public class ImportedFileValidator : AbstractValidator<ImportedFile>
{
    public ImportedFileValidator()
    {
        RuleFor(x => x.FileName)
            .NotEmpty().WithMessage("Имя файла не может быть пустым")
            .Must(name => !string.IsNullOrWhiteSpace(name)).WithMessage("Имя файла не может быть пустым или состоять только из пробелов");
    }
}
