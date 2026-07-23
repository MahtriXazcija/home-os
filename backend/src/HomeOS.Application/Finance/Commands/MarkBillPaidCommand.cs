using HomeOS.Application.Common;
using HomeOS.Domain.Finance;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HomeOS.Application.Finance.Commands;

public record MarkBillPaidCommand(Guid BillId, Guid PaidByUserId) : IRequest<BillDto>;

public class MarkBillPaidCommandHandler(IAppDbContext db) : IRequestHandler<MarkBillPaidCommand, BillDto>
{
    public async Task<BillDto> Handle(MarkBillPaidCommand request, CancellationToken cancellationToken)
    {
        var bill = await db.Bills.FirstOrDefaultAsync(b => b.Id == request.BillId, cancellationToken)
            ?? throw new InvalidOperationException("Bill not found.");

        var next = bill.NextDueDate();
        bill.MarkPaid(request.PaidByUserId);

        if (next is not null)
        {
            var recurring = Bill.Create(bill.HouseholdId, bill.Title, bill.Amount, bill.Category, next.Value, bill.CreatedByUserId, bill.Recurrence);
            db.Bills.Add(recurring);
        }

        await db.SaveChangesAsync(cancellationToken);
        return BillDto.From(bill);
    }
}
