using HomeOS.Application.Common;
using HomeOS.Domain.Chat;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HomeOS.Application.Chat.Commands;

public record MarkChatReadCommand(Guid HouseholdId, Guid UserId) : IRequest;

public class MarkChatReadCommandHandler(IAppDbContext db) : IRequestHandler<MarkChatReadCommand>
{
    public async Task Handle(MarkChatReadCommand request, CancellationToken cancellationToken)
    {
        var state = await db.ChatReadStates
            .FirstOrDefaultAsync(r => r.HouseholdId == request.HouseholdId && r.UserId == request.UserId, cancellationToken);

        if (state is null)
        {
            db.ChatReadStates.Add(ChatReadState.Create(request.HouseholdId, request.UserId));
        }
        else
        {
            state.MarkRead();
        }

        await db.SaveChangesAsync(cancellationToken);
    }
}
