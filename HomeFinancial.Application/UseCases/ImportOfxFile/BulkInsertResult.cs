namespace HomeFinancial.Application.UseCases.ImportOfxFile;

/// <summary>
/// Результаты операции массовой вставки транзакций
/// </summary>
/// <param name="Inserted">Количество успешно вставленных транзакций</param>
/// <param name="Duplicates">Количество пропущенных дубликатов</param>
public readonly record struct BulkInsertResult(int Inserted, int Duplicates);
