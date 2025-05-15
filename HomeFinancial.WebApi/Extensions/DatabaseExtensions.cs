using HomeFinancial.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HomeFinancial.WebApi.Extensions;

public static class DatabaseExtensions
{
    public static void ApplyDatabaseMigrations(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        try
        {
            logger.LogInformation("Применение миграций базы данных...");
            db.Database.Migrate();
            
            logger.LogInformation("Удаляем все файлы...");
            db.BankFiles.ExecuteDelete();
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Ошибка при применении миграций базы данных.");
            throw;
        }
    }

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
