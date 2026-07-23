using HomeOS.Application.Common;
using HomeOS.Domain.Finance;
using MediatR;

namespace HomeOS.Application.Finance.Commands;

public record CreateBillCommand(Guid HouseholdId, string Title, decimal Amount, string Category, DateTime DueDateUtc, Guid CreatedByUserId, BillRecurrence Recurrence) : IRequest<BillDto>;

public class CreateBillCommandHandler(IAppDbContext db) : IRequestHandler<CreateBillCommand, BillDto>
{
    public async Task<BillDto> Handle(CreateBillCommand request, CancellationToken cancellationToken)
    {
        var bill = Bill.Create(request.HouseholdId, request.Title, request.Amount, request.Category, request.DueDateUtc, request.CreatedByUserId, request.Recurrence);
        db.Bills.Add(bill);
        await db.SaveChangesAsync(cancellationToken);
        return BillDto.From(bill);
    }
}
