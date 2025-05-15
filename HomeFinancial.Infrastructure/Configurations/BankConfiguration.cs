using HomeFinancial.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HomeFinancial.Infrastructure.Configurations;

public class BankConfiguration : IEntityTypeConfiguration<Bank>
{
    public void Configure(EntityTypeBuilder<Bank> builder)
    {
        builder.HasKey(b => b.Id);

        builder.Property(b => b.BankId)
            .IsRequired()
            .HasMaxLength(32);

        builder.Property(b => b.Name)
            .HasMaxLength(256);

        builder.Property(b => b.Bic)
            .HasMaxLength(16);

        builder.Property(b => b.Swift)
            .HasMaxLength(16);

        builder.Property(b => b.Inn)
            .HasMaxLength(16);

        builder.Property(b => b.Kpp)
            .HasMaxLength(16);

        builder.Property(b => b.RegistrationNumber)
            .HasMaxLength(32);

        builder.Property(b => b.Address)
            .HasMaxLength(512);

        builder.HasIndex(b => b.BankId).IsUnique();
    }
}
