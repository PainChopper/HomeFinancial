using HomeFinancial.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HomeFinancial.Infrastructure.Configurations;

public class ImportedFileConfiguration : IEntityTypeConfiguration<BankFile>
{
    public void Configure(EntityTypeBuilder<BankFile> builder)
    {
        builder.HasKey(f => f.Id);
        builder.Metadata.GetProperties()
            .ToList()
            .ForEach(p => p.IsNullable = false);
        builder.Property(f => f.FileName)
            .HasMaxLength(255);
        builder.HasIndex(x => x.FileName)
            .IsUnique();
    }
}
