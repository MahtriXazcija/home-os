using HomeOS.Domain.Common;

namespace HomeOS.Domain.MealPlanner;

/// <summary>
/// The one genuinely new thing this app introduces — "which meal, which
/// day." Everything else it needs (a shopping task) already exists on the
/// platform; see CreateMealPlanEntryCommand for where that reuse actually
/// happens, not here.
/// </summary>
public class MealPlanEntry : AggregateRoot
{
    public Guid HouseholdId { get; private set; }
    public DateOnly MealDate { get; private set; }
    public MealType MealType { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public Guid CreatedByUserId { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    private MealPlanEntry() { }

    public static MealPlanEntry Create(Guid householdId, DateOnly mealDate, MealType mealType, string title, Guid createdByUserId)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("Meal title is required.", nameof(title));
        }

        var entry = new MealPlanEntry
        {
            HouseholdId = householdId,
            MealDate = mealDate,
            MealType = mealType,
            Title = title.Trim(),
            CreatedByUserId = createdByUserId,
            CreatedAtUtc = DateTime.UtcNow
        };

        entry.Raise(new MealPlannedEvent(entry.Id, householdId, mealDate, mealType, entry.Title));
        return entry;
    }
}
