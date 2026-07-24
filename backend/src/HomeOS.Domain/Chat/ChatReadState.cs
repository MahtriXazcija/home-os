using HomeOS.Domain.Common;

namespace HomeOS.Domain.Chat;

/// <summary>
/// One row per (household, user) — "everything up to this timestamp has
/// been read." Coarser than per-message read receipts, but it's exactly
/// what "seen by X" needs: a message is seen by X once X's LastReadAtUtc
/// passes that message's CreatedAtUtc.
/// </summary>
public class ChatReadState : Entity
{
    public Guid HouseholdId { get; private set; }
    public Guid UserId { get; private set; }
    public DateTime LastReadAtUtc { get; private set; }

    private ChatReadState() { }

    public static ChatReadState Create(Guid householdId, Guid userId) => new()
    {
        HouseholdId = householdId,
        UserId = userId,
        LastReadAtUtc = DateTime.UtcNow
    };

    public void MarkRead() => LastReadAtUtc = DateTime.UtcNow;
}
