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

    public DbSet<BankFile> BankFiles { get; set; } = null!;
    public DbSet<Bank> Banks { get; set; } = null!;
    public DbSet<BankAccount> BanksAccounts { get; set; } = null!;
    public DbSet<StatementEntry> StatementEntries { get; set; } = null!;
    public DbSet<EntryCategory> EntryCategories { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
