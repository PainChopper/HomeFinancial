using HomeFinancial.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HomeFinancial.Infrastructure.Configurations;

public class EntryCategoryConfiguration : IEntityTypeConfiguration<EntryCategory>
{
    public void Configure(EntityTypeBuilder<EntryCategory> builder)
    {
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(100);
        builder.HasIndex(c => c.Name).IsUnique();
    }
}
