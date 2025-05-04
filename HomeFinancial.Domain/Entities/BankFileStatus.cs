namespace HomeFinancial.Domain.Entities;

/// <summary>
/// Статус импортированного файла (отражает этап обработки)
/// </summary>
public enum BankFileStatus
{
    /// <summary>
    /// Файл в процессе обработки
    /// </summary>
    InProgress = 0,

    /// <summary>
    /// Файл успешно обработан
    /// </summary>
    Completed = 1,
}