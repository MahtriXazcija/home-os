using HomeOS.Application.Common;
using HomeOS.Domain.Finance;
using MediatR;

namespace HomeOS.Application.Finance.Commands;

public record CreateTransactionCommand(Guid HouseholdId, TransactionType Type, string Category, decimal Amount, DateTime OccurredAtUtc, Guid CreatedByUserId, string? Description) : IRequest<TransactionDto>;

public class CreateTransactionCommandHandler(IAppDbContext db) : IRequestHandler<CreateTransactionCommand, TransactionDto>
{
    public async Task<TransactionDto> Handle(CreateTransactionCommand request, CancellationToken cancellationToken)
    {
        var transaction = Transaction.Create(request.HouseholdId, request.Type, request.Category, request.Amount, request.OccurredAtUtc, request.CreatedByUserId, request.Description);
        db.Transactions.Add(transaction);
        await db.SaveChangesAsync(cancellationToken);
        return TransactionDto.From(transaction);
    }
}
