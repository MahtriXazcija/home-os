using HomeOS.Application.Common;
using HomeOS.Domain.MealPlanner;
using HomeOS.Domain.Tasks;
using MediatR;

namespace HomeOS.Application.MealPlanner.Commands;

public record CreateMealPlanEntryCommand(
    Guid HouseholdId,
    DateOnly MealDate,
    MealType MealType,
    string Title,
    Guid CreatedByUserId,
    bool AddShoppingTask) : IRequest<MealPlanEntryDto>;

/// <summary>
/// The "add shopping task" option is where the reuse the manifest promises
/// actually happens — it calls TaskItem.Create, the same factory the Tasks
/// module itself uses, instead of Meal Planner building its own task/list
/// concept. This is the exact scenario docs/app-sdk.md calls out by name.
/// </summary>
public class CreateMealPlanEntryCommandHandler(IAppDbContext db) : IRequestHandler<CreateMealPlanEntryCommand, MealPlanEntryDto>
{
    public async Task<MealPlanEntryDto> Handle(CreateMealPlanEntryCommand request, CancellationToken cancellationToken)
    {
        var entry = MealPlanEntry.Create(request.HouseholdId, request.MealDate, request.MealType, request.Title, request.CreatedByUserId);
        db.MealPlanEntries.Add(entry);

        if (request.AddShoppingTask)
        {
            var dueDateUtc = request.MealDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            var task = TaskItem.Create(
                request.HouseholdId, $"Buy ingredients for {entry.Title}", request.CreatedByUserId,
                dueDateUtc: dueDateUtc, tags: ["meal-planner"]);
            db.Tasks.Add(task);
        }

        await db.SaveChangesAsync(cancellationToken);
        return MealPlanEntryDto.From(entry);
    }
}
