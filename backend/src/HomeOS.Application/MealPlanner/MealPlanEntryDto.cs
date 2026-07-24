using HomeOS.Domain.MealPlanner;

namespace HomeOS.Application.MealPlanner;

public record MealPlanEntryDto(Guid Id, Guid HouseholdId, DateOnly MealDate, string MealType, string Title)
{
    public static MealPlanEntryDto From(MealPlanEntry e) => new(e.Id, e.HouseholdId, e.MealDate, e.MealType.ToString(), e.Title);
}
