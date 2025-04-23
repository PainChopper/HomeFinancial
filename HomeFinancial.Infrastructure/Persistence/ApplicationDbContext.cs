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
    public DbSet<ImportedFile> ImportedFiles { get; set; } = null!;
    public DbSet<BankTransaction> BankTransactions { get; set; } = null!;
    public DbSet<Category> Categories { get; set; } = null!;

    // Конфигурация моделей
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
