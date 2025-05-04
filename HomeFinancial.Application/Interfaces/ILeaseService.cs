namespace HomeFinancial.Application.Interfaces;

/// <summary>
/// Сервис для управления арендой (lease) обработки банковских файлов.
/// Использует Redis для захвата, продления и освобождения блокировок по имени файла.
/// </summary>
public interface ILeaseService
{
    /// <summary>
    /// Пытается захватить блокировку для файла с указанным временем аренды.
    /// </summary>
    /// <param name="fileName">Уникальное имя файла импорта.</param>
    /// <param name="leaseTime">Время действия блокировки (TTL).</param>
    /// <returns>Идентификатор аренды (leaseId).</returns>
    /// <exception cref="InvalidOperationException">Если файл уже обрабатывается другим процессом.</exception>
    Task<Guid> AcquireLeaseAsync(string fileName, TimeSpan leaseTime);

    /// <summary>
    /// Проверяет совпадение leaseId и продлевает время жизни блокировки.
    /// </summary>
    /// <param name="fileName">Уникальное имя файла импорта.</param>
    /// <param name="leaseId">Идентификатор аренды, полученный при захвате.</param>
    /// <param name="leaseTime">Новое время жизни блокировки (TTL).</param>
    /// <returns>True, если leaseId совпал и TTL продлён.</returns>
    /// <exception cref="InvalidOperationException">Если leaseId не совпал или блокировка истекла.</exception>
    Task ValidateAndExtendLeaseAsync(string fileName, Guid leaseId, TimeSpan leaseTime);

    /// <summary>
    /// Освобождает блокировку для файла при совпадении leaseId.
    /// </summary>
    /// <param name="fileName">Уникальное имя файла импорта.</param>
    /// <param name="leaseId">Идентификатор аренды, полученный при захвате.</param>
    /// <returns>Асинхронная операция.</returns>
    /// <exception cref="InvalidOperationException">Если невозможно освободить блокировку (leaseId не совпадает).</exception>
    Task ReleaseLeaseAsync(string fileName, Guid leaseId);
}