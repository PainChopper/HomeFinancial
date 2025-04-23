namespace HomeFinancial.Application.Common;

/// <summary>
/// Настройки импорта OFX-файлов (сопоставляется с ImportSettings из appsettings.json)
/// </summary>
public class ImportSettings
{
    /// <summary>
    /// Размер пакета для обработки транзакций
    /// </summary>
    public int BatchSize { get; set; } = 100;
}
