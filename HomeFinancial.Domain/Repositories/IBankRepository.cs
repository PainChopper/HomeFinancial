using HomeFinancial.Domain.Entities;

namespace HomeFinancial.Domain.Repositories;

/// <summary>
/// Интерфейс репозитория для работы с банками
/// </summary>
public interface IBankRepository : IGenericRepository<Bank>
{
    /// <summary>
    /// Получает или создаёт запись о банке по его идентификатору
    /// </summary>
    /// <param name="bankId">Идентификатор банка</param>
    /// <param name="bankName">Название банка</param>
    /// <param name="ct">Токен отмены</param>
    /// <returns>Сущность банка</returns>
    Task<Bank> GetOrCreateAsync(string bankId, string bankName, CancellationToken ct);
}
