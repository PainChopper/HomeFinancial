namespace HomeFinancial.Application.UseCases.ImportOfxFile;

/// <summary>
/// Метрики, полученные при обработке одного банковского statement-а
/// </summary>
/// <param name="Inserted">Вставлено новых транзакций</param>
/// <param name="Duplicates">Пропущено дубликатов</param>
/// <param name="Errors">Пропущено из-за ошибок валидации</param>
public readonly record struct StatementProcessMetrics(
    int Inserted,
    int Duplicates,
    int Errors)
{
    /// <summary>
    /// Общее количество обработанных транзакций
    /// </summary>
    public int Total => Inserted + Duplicates + Errors;
    
    public static StatementProcessMetrics operator +(
        StatementProcessMetrics left,
        StatementProcessMetrics right) =>
        new(
            left.Inserted + right.Inserted,
            left.Duplicates + right.Duplicates,
            left.Errors + right.Errors);
}
