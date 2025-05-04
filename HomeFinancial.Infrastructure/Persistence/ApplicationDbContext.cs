using HomeFinancial.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HomeFinancial.Infrastructure.Persistence;

/// <summary>
/// Контекст данных приложения.
/// </summary>
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// Импортированные файлы
    /// </summary>
    public DbSet<BankFile> BankFiles { get; set; } = null!;
    public DbSet<FileTransaction> FileTransactions { get; set; } = null!;
    public DbSet<TransactionCategory> TransactionCategories { get; set; } = null!;

    // Конфигурация моделей
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
