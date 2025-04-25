using HomeFinancial.Domain.Entities;
using HomeFinancial.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HomeFinancial.Infrastructure.Configurations;

public class ImportedFileConfiguration : IEntityTypeConfiguration<ImportedFile>
{
    public void Configure(EntityTypeBuilder<ImportedFile> builder)
    {
        builder.HasKey(f => f.Id);
        builder.SetAllPropertiesRequired();
        builder.Property(f => f.FileName).HasMaxLength(255);
    }
}
