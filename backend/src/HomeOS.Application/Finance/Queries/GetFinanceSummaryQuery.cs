using HomeOS.Application.Common;
using HomeOS.Domain.Finance;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HomeOS.Application.Finance.Queries;

public record GetFinanceSummaryQuery(Guid HouseholdId) : IRequest<FinanceSummaryDto>;

public class GetFinanceSummaryQueryHandler(IAppDbContext db) : IRequestHandler<GetFinanceSummaryQuery, FinanceSummaryDto>
{
    public async Task<FinanceSummaryDto> Handle(GetFinanceSummaryQuery request, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var monthEnd = monthStart.AddMonths(1);

        var monthTransactions = await db.Transactions
            .Where(t => t.HouseholdId == request.HouseholdId && t.OccurredAtUtc >= monthStart && t.OccurredAtUtc < monthEnd)
            .ToListAsync(cancellationToken);

        var totalIncome = monthTransactions.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount);
        var totalExpense = monthTransactions.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount);

        var paidBillsThisMonth = await db.Bills
            .Where(b => b.HouseholdId == request.HouseholdId && b.IsPaid && b.PaidAtUtc >= monthStart && b.PaidAtUtc < monthEnd)
            .ToListAsync(cancellationToken);

        var spendByMember = monthTransactions
            .Where(t => t.Type == TransactionType.Expense)
            .GroupBy(t => t.CreatedByUserId)
            .ToDictionary(g => g.Key, g => g.Sum(t => t.Amount));

        foreach (var bill in paidBillsThisMonth.Where(b => b.PaidByUserId is not null))
        {
            spendByMember[bill.PaidByUserId!.Value] = spendByMember.GetValueOrDefault(bill.PaidByUserId!.Value) + bill.Amount;
        }

        var budgets = await db.Budgets.Where(b => b.HouseholdId == request.HouseholdId).ToListAsync(cancellationToken);
        var spentByCategory = monthTransactions
            .Where(t => t.Type == TransactionType.Expense)
            .GroupBy(t => t.Category)
            .ToDictionary(g => g.Key, g => g.Sum(t => t.Amount));

        var budgetDtos = budgets
            .Select(b => BudgetDto.From(b, spentByCategory.GetValueOrDefault(b.Category)))
            .ToList();

        return new FinanceSummaryDto(
            totalIncome,
            totalExpense,
            spendByMember.Select(kv => new MemberSpendDto(kv.Key, kv.Value)).ToList(),
            budgetDtos);
    }
}
