using HomeFinancial.Application.Common.Abstractions;

namespace HomeFinancial.WebApi;

public static class DatabaseExtensions
{
    /// <summary>
    /// Проверяет соединение с базой данных через IDatabaseHealthChecker.
    /// </summary>
    /// <param name="services">Провайдер сервисов приложения</param>
    public static async Task<bool> CheckDatabaseConnectionAsync(this IServiceProvider services)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        using var scope = services.CreateScope();
        var provider = scope.ServiceProvider;
        var dbChecker = provider.GetRequiredService<IDatabaseHealthChecker>();

        logger.LogInformation("Проверка соединения с базой данных.");

        if (await dbChecker.CheckDatabaseHealthAsync())
            return true;

        logger.LogCritical("Не удалось установить соединение с базой данных. Проверьте строку подключения и доступность сервера БД.");
        return false;
    }
}
