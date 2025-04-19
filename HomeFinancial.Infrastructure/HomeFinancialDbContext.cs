using HomeFinancial.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HomeFinancial.Infrastructure;

public class HomeFinancialDbContext : DbContext
{
    public HomeFinancialDbContext(DbContextOptions<HomeFinancialDbContext> options)
        : base(options)
    {
    }

    public DbSet<BankTransaction> BankTransactions => Set<BankTransaction>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<ImportedFile> ImportedFiles => Set<ImportedFile>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Уникальность FITID
        modelBuilder.Entity<BankTransaction>()
            .HasIndex(t => t.FitId)
            .IsUnique();

        // Уникальность имени файла
        modelBuilder.Entity<ImportedFile>()
            .HasIndex(f => f.FileName)
            .IsUnique();

        // Категория: хотим игнорировать регистр.
        // Самый верный способ на PostgreSQL — использовать citext или индекс по LOWER(Name).
        // Но для простоты укажем IsUnique, а в коде будем сами проверять.
        modelBuilder.Entity<Category>()
            .HasIndex(c => c.Name)
            .IsUnique();
    }
}