using HomeOS.Application.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HomeOS.Application.MealPlanner.Commands;

public record DeleteMealPlanEntryCommand(Guid EntryId) : IRequest;

public class DeleteMealPlanEntryCommandHandler(IAppDbContext db) : IRequestHandler<DeleteMealPlanEntryCommand>
{
    public async Task Handle(DeleteMealPlanEntryCommand request, CancellationToken cancellationToken)
    {
        var entry = await db.MealPlanEntries.FirstOrDefaultAsync(e => e.Id == request.EntryId, cancellationToken);
        if (entry is null) return;

        db.MealPlanEntries.Remove(entry);
        await db.SaveChangesAsync(cancellationToken);
    }
}
