namespace HomeFinancial.Infrastructure.Persistence;

/// <summary>
/// Строки подключения к различным базам данных
/// </summary>
public record ConnectionStrings
{
    /// <summary>
    /// Строка подключения к PostgreSQL
    /// </summary>
    public required string Postgres { get; init; }

    /// <summary>
    /// Строка подключения к Redis
    /// </summary>
    public required string Redis { get; init; }
}
