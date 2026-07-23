using HomeOS.Domain.Common;

namespace HomeOS.Domain.Finance;

public class Bill : AggregateRoot
{
    public Guid HouseholdId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public decimal Amount { get; private set; }
    public string Category { get; private set; } = string.Empty;
    public DateTime DueDateUtc { get; private set; }
    public BillRecurrence Recurrence { get; private set; }
    public bool IsPaid { get; private set; }
    public Guid? PaidByUserId { get; private set; }
    public DateTime? PaidAtUtc { get; private set; }
    public Guid CreatedByUserId { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    private Bill() { }

    public static Bill Create(Guid householdId, string title, decimal amount, string category, DateTime dueDateUtc, Guid createdByUserId, BillRecurrence recurrence = BillRecurrence.None)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("Bill title is required.", nameof(title));
        }
        if (amount <= 0)
        {
            throw new ArgumentException("Amount must be positive.", nameof(amount));
        }

        var bill = new Bill
        {
            HouseholdId = householdId,
            Title = title.Trim(),
            Amount = amount,
            Category = category.Trim(),
            DueDateUtc = dueDateUtc,
            Recurrence = recurrence,
            CreatedByUserId = createdByUserId,
            CreatedAtUtc = DateTime.UtcNow
        };

        bill.Raise(new BillCreatedEvent(bill.Id, householdId, bill.Title, amount, dueDateUtc, createdByUserId));
        return bill;
    }

    public void MarkPaid(Guid paidByUserId)
    {
        IsPaid = true;
        PaidByUserId = paidByUserId;
        PaidAtUtc = DateTime.UtcNow;
    }

    public DateTime? NextDueDate() => Recurrence switch
    {
        BillRecurrence.Monthly => DueDateUtc.AddMonths(1),
        BillRecurrence.Yearly => DueDateUtc.AddYears(1),
        _ => null
    };
}
