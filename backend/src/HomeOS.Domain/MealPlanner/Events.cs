using HomeOS.Domain.Common;

namespace HomeOS.Domain.MealPlanner;

/// <summary>
/// Nothing subscribes to this yet — that's the point. Publishing it costs
/// nothing (MediatR no-ops with zero handlers) and means a future app can
/// react to meals being planned without Meal Planner ever being touched
/// again, matching "a future app can build on it, without anyone planning
/// for it in advance" (docs/app-sdk.md §2).
/// </summary>
public record MealPlannedEvent(Guid EntryId, Guid HouseholdId, DateOnly MealDate, MealType MealType, string Title) : IDomainEvent
{
    public DateTime OccurredAtUtc { get; } = DateTime.UtcNow;
}
