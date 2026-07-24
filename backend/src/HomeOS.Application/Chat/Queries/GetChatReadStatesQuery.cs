using HomeOS.Application.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HomeOS.Application.Chat.Queries;

public record GetChatReadStatesQuery(Guid HouseholdId) : IRequest<List<ChatReadStateDto>>;

public class GetChatReadStatesQueryHandler(IAppDbContext db) : IRequestHandler<GetChatReadStatesQuery, List<ChatReadStateDto>>
{
    public async Task<List<ChatReadStateDto>> Handle(GetChatReadStatesQuery request, CancellationToken cancellationToken)
    {
        var states = await db.ChatReadStates
            .Where(r => r.HouseholdId == request.HouseholdId)
            .ToListAsync(cancellationToken);

        return [.. states.Select(ChatReadStateDto.From)];
    }
}
