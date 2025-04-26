using HomeFinancial.Infrastructure.Persistence;

namespace HomeFinancial.WebApi;

public static class DatabaseExtensions
{
    /// <summary>
    /// Проверяет соединение с базой данных через ApplicationDbContext.
    /// </summary>
    /// <param name="services">Провайдер сервисов приложения</param>
    public static async Task<bool> CheckDatabaseConnectionAsync(this IServiceProvider services)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        using var scope = services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        logger.LogInformation("Проверка соединения с базой данных.");

        if (await dbContext.Database.CanConnectAsync())
            return true;

        logger.LogCritical("Не удалось установить соединение с базой данных. Проверьте строку подключения и доступность сервера БД.");
        return false;
    }
}
