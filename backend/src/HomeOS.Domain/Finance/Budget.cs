using HomeOS.Domain.Common;

namespace HomeOS.Domain.Finance;

public class Budget : Entity
{
    public Guid HouseholdId { get; private set; }
    public string Category { get; private set; } = string.Empty;
    public decimal MonthlyLimit { get; private set; }

    private Budget() { }

    public static Budget Create(Guid householdId, string category, decimal monthlyLimit)
    {
        if (string.IsNullOrWhiteSpace(category))
        {
            throw new ArgumentException("Category is required.", nameof(category));
        }

        return new Budget { HouseholdId = householdId, Category = category.Trim(), MonthlyLimit = monthlyLimit };
    }

    public void SetLimit(decimal monthlyLimit) => MonthlyLimit = monthlyLimit;
}
