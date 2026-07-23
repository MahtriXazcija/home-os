using HomeOS.Application.Common;
using HomeOS.Domain.Finance;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HomeOS.Application.Finance.Commands;

public record SetBudgetCommand(Guid HouseholdId, string Category, decimal MonthlyLimit) : IRequest;

public class SetBudgetCommandHandler(IAppDbContext db) : IRequestHandler<SetBudgetCommand>
{
    public async Task Handle(SetBudgetCommand request, CancellationToken cancellationToken)
    {
        var budget = await db.Budgets
            .FirstOrDefaultAsync(b => b.HouseholdId == request.HouseholdId && b.Category == request.Category, cancellationToken);

        if (budget is null)
        {
            db.Budgets.Add(Budget.Create(request.HouseholdId, request.Category, request.MonthlyLimit));
        }
        else
        {
            budget.SetLimit(request.MonthlyLimit);
        }

        await db.SaveChangesAsync(cancellationToken);
    }
}
