using HomeOS.Application.Common;
using HomeOS.Application.Households;
using HomeOS.Application.Notifications;
using HomeOS.Domain.Chat;
using HomeOS.Domain.Notifications;
using MediatR;

namespace HomeOS.Application.Chat.Commands;

public record SendChatMessageCommand(Guid HouseholdId, Guid SenderUserId, string Content) : IRequest<ChatMessageDto>;

public class SendChatMessageCommandHandler(
    IAppDbContext db,
    IHouseholdRepository householdRepository,
    IUserDirectory userDirectory,
    INotificationDispatcher notificationDispatcher) : IRequestHandler<SendChatMessageCommand, ChatMessageDto>
{
    public async Task<ChatMessageDto> Handle(SendChatMessageCommand request, CancellationToken cancellationToken)
    {
        var message = ChatMessage.Create(request.HouseholdId, request.SenderUserId, request.Content);
        db.ChatMessages.Add(message);
        await db.SaveChangesAsync(cancellationToken);

        var household = await householdRepository.GetByIdAsync(request.HouseholdId, cancellationToken);
        if (household is not null)
        {
            var sender = await userDirectory.GetContactAsync(request.SenderUserId, cancellationToken);
            var senderName = sender?.DisplayName ?? "Someone";
            var preview = request.Content.Length > 80 ? request.Content[..80] + "…" : request.Content;

            foreach (var member in household.Members.Where(m => m.UserId != request.SenderUserId))
            {
                await notificationDispatcher.DispatchAsync(
                    member.UserId, request.HouseholdId, NotificationCategory.ChatMessage,
                    $"{senderName} sent a message", preview, cancellationToken);
            }
        }

        return ChatMessageDto.From(message);
    }
}
