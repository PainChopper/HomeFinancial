using HomeFinancial.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HomeFinancial.Infrastructure.Persistence.Configurations;

public class BankTransactionConfiguration : IEntityTypeConfiguration<BankTransaction>
{
    public void Configure(EntityTypeBuilder<BankTransaction> builder)
    {
        builder.HasKey(t => t.Id);
        builder.Property(t => t.FitId).IsRequired();
        builder.Property(t => t.Amount).IsRequired();
        builder.Property(t => t.Date).IsRequired();
        builder.Property(t => t.Description).IsRequired();
        // builder.Property(t => t.CategoryId).IsRequired();
    }
}
