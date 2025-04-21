using HomeFinancial.Infrastructure;
using HomeFinancial.Infrastructure.Persistence;

namespace HomeFinancial.WebApi;

public static class DatabaseExtensions
{
    /// <summary>
    /// Проверяет соединение с базой данных и возвращает true при успешном соединении, false в противном случае.
    /// </summary>
    /// <param name="services">Провайдер сервисов приложения</param>
    public static bool CheckDatabaseConnection(this IServiceProvider services)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        using var scope = services.CreateScope();
        var provider = scope.ServiceProvider;
        var dbContext = provider.GetRequiredService<ApplicationDbContext>();

        logger.LogInformation("Проверка соединения с базой данных.");

        if (dbContext.Database.CanConnect())
            return true;

        logger.LogCritical("Не удалось установить соединение с базой данных. Проверьте строку подключения и доступность сервера БД.");
        return false;
    }
}
