using HomeOS.Application.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HomeOS.Application.Finance.Queries;

public record GetTransactionsQuery(Guid HouseholdId) : IRequest<List<TransactionDto>>;

public class GetTransactionsQueryHandler(IAppDbContext db) : IRequestHandler<GetTransactionsQuery, List<TransactionDto>>
{
    public async Task<List<TransactionDto>> Handle(GetTransactionsQuery request, CancellationToken cancellationToken)
    {
        var transactions = await db.Transactions
            .Where(t => t.HouseholdId == request.HouseholdId)
            .OrderByDescending(t => t.OccurredAtUtc)
            .Take(200)
            .ToListAsync(cancellationToken);

        return transactions.Select(TransactionDto.From).ToList();
    }
}
