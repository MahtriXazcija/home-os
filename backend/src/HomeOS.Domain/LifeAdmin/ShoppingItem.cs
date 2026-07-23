using HomeOS.Domain.Common;

namespace HomeOS.Domain.LifeAdmin;

public class ShoppingItem : Entity
{
    public Guid HouseholdId { get; private set; }
    public string Text { get; private set; } = string.Empty;
    public bool IsChecked { get; private set; }
    public Guid CreatedByUserId { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    private ShoppingItem() { }

    public static ShoppingItem Create(Guid householdId, string text, Guid createdByUserId)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new ArgumentException("Item text is required.", nameof(text));
        }

        return new ShoppingItem
        {
            HouseholdId = householdId,
            Text = text.Trim(),
            CreatedByUserId = createdByUserId,
            CreatedAtUtc = DateTime.UtcNow
        };
    }

    public void SetChecked(bool isChecked) => IsChecked = isChecked;
}
