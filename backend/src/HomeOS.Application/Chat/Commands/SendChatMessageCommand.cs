using HomeOS.Application.Common;
using HomeOS.Domain.Chat;
using MediatR;

namespace HomeOS.Application.Chat.Commands;

public record SendChatMessageCommand(Guid HouseholdId, Guid SenderUserId, string Content) : IRequest<ChatMessageDto>;

public class SendChatMessageCommandHandler(IAppDbContext db) : IRequestHandler<SendChatMessageCommand, ChatMessageDto>
{
    public async Task<ChatMessageDto> Handle(SendChatMessageCommand request, CancellationToken cancellationToken)
    {
        var message = ChatMessage.Create(request.HouseholdId, request.SenderUserId, request.Content);
        db.ChatMessages.Add(message);
        await db.SaveChangesAsync(cancellationToken);

        return ChatMessageDto.From(message);
    }
}
