using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace HomeFinancial.Infrastructure;

public class HomeFinancialDbContextFactory : IDesignTimeDbContextFactory<HomeFinancialDbContext>
{
    public HomeFinancialDbContext CreateDbContext(string[] args)
    {
        // Создание экземпляра ConfigurationBuilder
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory()) // Указывает текущую директорию
            .AddUserSecrets<HomeFinancialDbContextFactory>() // Подключение User Secrets
            .Build();

        // Получение строки подключения
        var connectionString = configuration.GetConnectionString("HomeFinancialConnection");
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException("Connection string 'HomeFinancialConnection' not found in user secrets.");
        }

        // Настройка DbContextOptionsBuilder
        var optionsBuilder = new DbContextOptionsBuilder<HomeFinancialDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new HomeFinancialDbContext(optionsBuilder.Options);
    }
}