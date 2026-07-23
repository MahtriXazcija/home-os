using HomeOS.Domain.Common;

namespace HomeOS.Domain.Households;

public class HouseholdInvitation : Entity
{
    public Guid HouseholdId { get; private set; }
    public string Email { get; private set; } = string.Empty;
    public string Token { get; private set; } = string.Empty;
    public Guid InvitedByUserId { get; private set; }
    public InvitationStatus Status { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime ExpiresAtUtc { get; private set; }

    private HouseholdInvitation() { }

    internal HouseholdInvitation(Guid householdId, string email, Guid invitedByUserId)
    {
        HouseholdId = householdId;
        Email = email.Trim().ToLowerInvariant();
        Token = Guid.NewGuid().ToString("N");
        InvitedByUserId = invitedByUserId;
        Status = InvitationStatus.Pending;
        CreatedAtUtc = DateTime.UtcNow;
        ExpiresAtUtc = CreatedAtUtc.AddDays(7);
    }

    public bool IsUsable => Status == InvitationStatus.Pending && ExpiresAtUtc > DateTime.UtcNow;

    internal void MarkAccepted() => Status = InvitationStatus.Accepted;
}
