using HomeOS.Application.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HomeOS.Application.Chat.Queries;

/// <summary>Most recent messages first from the DB, then reversed so the UI can just append and scroll down.</summary>
public record GetChatMessagesQuery(Guid HouseholdId, int Take = 100) : IRequest<List<ChatMessageDto>>;

public class GetChatMessagesQueryHandler(IAppDbContext db) : IRequestHandler<GetChatMessagesQuery, List<ChatMessageDto>>
{
    public async Task<List<ChatMessageDto>> Handle(GetChatMessagesQuery request, CancellationToken cancellationToken)
    {
        var messages = await db.ChatMessages
            .Where(m => m.HouseholdId == request.HouseholdId)
            .OrderByDescending(m => m.CreatedAtUtc)
            .Take(request.Take)
            .ToListAsync(cancellationToken);

        messages.Reverse();
        return [.. messages.Select(ChatMessageDto.From)];
    }
}
