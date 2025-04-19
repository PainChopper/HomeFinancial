using HomeFinancial.Infrastructure;

namespace HomeFinancial.WebApi;

public static class DatabaseExtensions
{
    /// <summary>
    /// Проверяет соединение с базой данных и выбрасывает исключение при ошибке.
    /// </summary>
    /// <param name="services">Провайдер сервисов приложения</param>
    /// <param name="logger">Логгер для вывода сообщений</param>
    public static void CheckDatabaseConnection(this IServiceProvider services, ILogger logger)
    {
        using var scope = services.CreateScope();
        var provider = scope.ServiceProvider;
        var dbContext = provider.GetRequiredService<HomeFinancialDbContext>();

        logger.LogInformation("Проверка соединения с базой данных.");

        if (dbContext.Database.CanConnect()) 
            return;

        logger.LogCritical("Не удалось установить соединение с базой данных. Проверьте строку подключения и доступность сервера БД.");
        Environment.Exit(1);
    }
}
