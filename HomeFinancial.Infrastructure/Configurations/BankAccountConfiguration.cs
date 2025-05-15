using HomeFinancial.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HomeFinancial.Infrastructure.Configurations;

public class BankAccountConfiguration : IEntityTypeConfiguration<BankAccount>
{
    public void Configure(EntityTypeBuilder<BankAccount> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.AccountId)
            .IsRequired()
            .HasMaxLength(32);

        builder.Property(x => x.AccountType)
            .IsRequired()
            .HasMaxLength(32);

        builder.HasOne(x => x.Bank)
            .WithMany()
            .HasForeignKey(x => x.BankId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.AccountId).IsUnique(false);
    }
}
