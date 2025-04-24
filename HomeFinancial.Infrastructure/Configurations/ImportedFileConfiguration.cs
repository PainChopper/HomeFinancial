using HomeFinancial.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HomeFinancial.Infrastructure.Configurations;

public class ImportedFileConfiguration : IEntityTypeConfiguration<ImportedFile>
{
    public void Configure(EntityTypeBuilder<ImportedFile> builder)
    {
        builder.HasKey(f => f.Id);
        builder.Property(f => f.FileName).IsRequired().HasMaxLength(255);
        builder.Property(f => f.ImportedAt).IsRequired();
        builder.Property(f => f.Status).IsRequired();
    }
}
