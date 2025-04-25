using HomeFinancial.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HomeFinancial.Infrastructure.Configurations;

public class ImportedFileConfiguration : IEntityTypeConfiguration<ImportedFile>
{
    public void Configure(EntityTypeBuilder<ImportedFile> builder)
    {
        builder.HasKey(f => f.Id);
        builder.Metadata.GetProperties()
            .ToList()
            .ForEach(p => p.IsNullable = false);
        builder.Property(f => f.FileName)
            .HasMaxLength(255);
        builder.Metadata.GetProperties().ToList().ForEach(p => p.IsNullable = false);
    }
}
