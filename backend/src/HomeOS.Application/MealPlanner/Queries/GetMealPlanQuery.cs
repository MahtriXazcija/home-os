using HomeOS.Application.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HomeOS.Application.MealPlanner.Queries;

public record GetMealPlanQuery(Guid HouseholdId, DateOnly FromDate, DateOnly ToDate) : IRequest<List<MealPlanEntryDto>>;

public class GetMealPlanQueryHandler(IAppDbContext db) : IRequestHandler<GetMealPlanQuery, List<MealPlanEntryDto>>
{
    public async Task<List<MealPlanEntryDto>> Handle(GetMealPlanQuery request, CancellationToken cancellationToken)
    {
        var entries = await db.MealPlanEntries
            .Where(e => e.HouseholdId == request.HouseholdId && e.MealDate >= request.FromDate && e.MealDate <= request.ToDate)
            .OrderBy(e => e.MealDate)
            .ToListAsync(cancellationToken);

        return entries.Select(MealPlanEntryDto.From).ToList();
    }
}
