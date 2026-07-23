using HomeOS.Domain.Common;

namespace HomeOS.Domain.Finance;

public class Transaction : AggregateRoot
{
    public Guid HouseholdId { get; private set; }
    public TransactionType Type { get; private set; }
    public string Category { get; private set; } = string.Empty;
    public decimal Amount { get; private set; }
    public DateTime OccurredAtUtc { get; private set; }
    public string? Description { get; private set; }
    public Guid CreatedByUserId { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    private Transaction() { }

    public static Transaction Create(Guid householdId, TransactionType type, string category, decimal amount, DateTime occurredAtUtc, Guid createdByUserId, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(category))
        {
            throw new ArgumentException("Category is required.", nameof(category));
        }
        if (amount <= 0)
        {
            throw new ArgumentException("Amount must be positive.", nameof(amount));
        }

        return new Transaction
        {
            HouseholdId = householdId,
            Type = type,
            Category = category.Trim(),
            Amount = amount,
            OccurredAtUtc = occurredAtUtc,
            Description = description,
            CreatedByUserId = createdByUserId,
            CreatedAtUtc = DateTime.UtcNow
        };
    }
}
