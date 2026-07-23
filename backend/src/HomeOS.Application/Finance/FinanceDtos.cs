using HomeOS.Domain.Finance;

namespace HomeOS.Application.Finance;

public record TransactionDto(Guid Id, Guid HouseholdId, string Type, string Category, decimal Amount, DateTime OccurredAtUtc, string? Description, Guid CreatedByUserId)
{
    public static TransactionDto From(Transaction t) => new(t.Id, t.HouseholdId, t.Type.ToString(), t.Category, t.Amount, t.OccurredAtUtc, t.Description, t.CreatedByUserId);
}

public record BillDto(Guid Id, Guid HouseholdId, string Title, decimal Amount, string Category, DateTime DueDateUtc, string Recurrence, bool IsPaid, Guid? PaidByUserId, DateTime? PaidAtUtc)
{
    public static BillDto From(Bill b) => new(b.Id, b.HouseholdId, b.Title, b.Amount, b.Category, b.DueDateUtc, b.Recurrence.ToString(), b.IsPaid, b.PaidByUserId, b.PaidAtUtc);
}

public record BudgetDto(Guid Id, string Category, decimal MonthlyLimit, decimal SpentThisMonth)
{
    public static BudgetDto From(Budget b, decimal spent) => new(b.Id, b.Category, b.MonthlyLimit, spent);
}

public record FinanceSummaryDto(decimal TotalIncomeThisMonth, decimal TotalExpenseThisMonth, List<MemberSpendDto> SpendByMember, List<BudgetDto> Budgets);

public record MemberSpendDto(Guid UserId, decimal PaidThisMonth);
