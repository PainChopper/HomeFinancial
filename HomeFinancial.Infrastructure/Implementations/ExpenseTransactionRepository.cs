using HomeFinancial.Infrastructure.Persistence;

namespace HomeFinancial.Infrastructure.Implementations;

public class ExpenseTransactionRepository : BankTransactionRepositoryBase<Domain.Entities.ExpenseTransaction>
{
    public ExpenseTransactionRepository(ApplicationDbContext context)
        : base(context, context.ExpenseTransactions) { }
}
