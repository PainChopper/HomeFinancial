using HomeFinancial.Domain.Entities;
using HomeFinancial.Infrastructure.Persistence;

namespace HomeFinancial.Infrastructure.Implementations;

public class IncomeTransactionRepository(ApplicationDbContext context)
    : BankTransactionRepositoryBase<IncomeTransaction>(context, context.IncomeTransactions);
