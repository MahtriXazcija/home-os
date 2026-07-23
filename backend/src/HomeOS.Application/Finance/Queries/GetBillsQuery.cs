using HomeOS.Application.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HomeOS.Application.Finance.Queries;

public record GetBillsQuery(Guid HouseholdId) : IRequest<List<BillDto>>;

public class GetBillsQueryHandler(IAppDbContext db) : IRequestHandler<GetBillsQuery, List<BillDto>>
{
    public async Task<List<BillDto>> Handle(GetBillsQuery request, CancellationToken cancellationToken)
    {
        var bills = await db.Bills
            .Where(b => b.HouseholdId == request.HouseholdId && !b.IsPaid)
            .OrderBy(b => b.DueDateUtc)
            .ToListAsync(cancellationToken);

        return bills.Select(BillDto.From).ToList();
    }
}
