using HomeFinancial.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HomeFinancial.WebApi.Extensions;

public static class DatabaseMigrationExtensions
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
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Ошибка при применении миграций базы данных.");
            throw; // Прерываем запуск, чтобы не работать на неконсистентной схеме
        }
    }
}
