using HomeFinancial.Domain.Entities;

namespace HomeFinancial.Domain.Repositories;

/// <summary>
/// Интерфейс репозитория для работы с банковскими счетами
/// </summary>
public interface IBankAccountRepository : IGenericRepository<BankAccount>
{
    /// <summary>
    /// Получает или создаёт запись о банковском счёте
    /// </summary>
    /// <param name="bankId">Идентификатор банка</param>
    /// <param name="accountId">Идентификатор счёта</param>
    /// <param name="accountType">Тип счёта</param>
    /// <param name="ct">Токен отмены</param>
    /// <returns>Сущность банковского счёта</returns>
    Task<BankAccount> GetOrCreateAsync(int bankId, string accountId, string accountType, CancellationToken ct);
}
