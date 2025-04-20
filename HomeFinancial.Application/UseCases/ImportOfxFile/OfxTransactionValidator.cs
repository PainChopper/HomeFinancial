using System.Collections.Generic;
using HomeFinancial.OfxParser;

namespace HomeFinancial.Application.UseCases.ImportOfxFile
{
    /// <summary>
    /// Валидатор OFX-транзакции для импорта
    /// </summary>
    public class TransactionValidationResult
    {
        public bool IsValid => Errors.Count == 0;
        public List<string> Errors { get; } = new();
    }

    public static class OfxTransactionValidator
    {
        public static TransactionValidationResult Validate(OfxTransaction t)
        {
            var result = new TransactionValidationResult();

            if (t.Amount == null)
                result.Errors.Add($"Пропущена транзакция без суммы: {t.TranId}");
            if (string.IsNullOrWhiteSpace(t.Description))
                result.Errors.Add($"Пропущена транзакция без описания: {t.TranId}");
            if (string.IsNullOrWhiteSpace(t.Category))
                result.Errors.Add($"Пропущена транзакция без категории: {t.TranId}");

            return result;
        }
    }
}
