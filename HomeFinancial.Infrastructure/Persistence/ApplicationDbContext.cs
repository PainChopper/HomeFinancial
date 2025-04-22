using HomeFinancial.Domain.Entities;
using HomeFinancial.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HomeFinancial.Infrastructure.Persistence;

/// <summary>
/// Контекст данных приложения с поддержкой Identity.
/// </summary>
public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole<long>, long>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// Импортированные файлы
    /// </summary>
    public DbSet<ImportedFile> ImportedFiles { get; set; } = null!;

    // Конфигурация моделей
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        // Пример: связь ImportedFile.UserId <-> ApplicationUser.Id
        builder.Entity<ImportedFile>()
            .HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(f => f.UserId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

    }
}
