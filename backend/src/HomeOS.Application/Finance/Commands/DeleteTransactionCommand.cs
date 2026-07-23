using HomeOS.Application.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HomeOS.Application.Finance.Commands;

public record DeleteTransactionCommand(Guid TransactionId) : IRequest;

public class DeleteTransactionCommandHandler(IAppDbContext db) : IRequestHandler<DeleteTransactionCommand>
{
    public async Task Handle(DeleteTransactionCommand request, CancellationToken cancellationToken)
    {
        var transaction = await db.Transactions.FirstOrDefaultAsync(t => t.Id == request.TransactionId, cancellationToken);
        if (transaction is null) return;

        db.Transactions.Remove(transaction);
        await db.SaveChangesAsync(cancellationToken);
    }
}
