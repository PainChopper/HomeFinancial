using HomeFinancial.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HomeFinancial.Infrastructure.Configurations;

public class StatementEntryConfiguration : IEntityTypeConfiguration<StatementEntry>
{
    public void Configure(EntityTypeBuilder<StatementEntry> builder)
    {
        builder.HasKey(t => t.Id);
        builder.Metadata.GetProperties()
            .ToList()
            .ForEach(t => t.IsNullable = false);
        builder.HasOne(t => t.Category)
            .WithMany()
            .HasForeignKey(t => t.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(t => t.FitId).IsUnique();
        
        // Еще хорошо было бы партицую по FileId задать, чтобы удалять транзакции было сразу файлами без напряжения сервера
        // PARTITION BY LIST (file_id)
        // Есть пара решений:
        // 1. Можно NpgsqlMigrationsSqlGenerator переопределить и использовать на этапе миграции, чтобы дописать DDL SQL
        // 2. Можно использовать https://github.com/pgpartman/pg_partman, вытащить DDL таблицы из БД,
        // добавить туда PARTITION BY LIST (file_id) и пересоздать таблицу. 
    }
}
