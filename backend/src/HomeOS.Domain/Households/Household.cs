using HomeOS.Domain.Common;

namespace HomeOS.Domain.Households;

public class Household : AggregateRoot
{
    private readonly List<HouseholdMember> _members = [];
    private readonly List<HouseholdInvitation> _invitations = [];

    public string Name { get; private set; } = string.Empty;
    public Guid CreatedByUserId { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    public IReadOnlyList<HouseholdMember> Members => _members.AsReadOnly();
    public IReadOnlyList<HouseholdInvitation> Invitations => _invitations.AsReadOnly();

    private Household() { }

    public static Household Create(string name, Guid creatorUserId, string creatorDisplayName)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Household name is required.", nameof(name));
        }

        var household = new Household
        {
            Name = name.Trim(),
            CreatedByUserId = creatorUserId,
            CreatedAtUtc = DateTime.UtcNow
        };

        household._members.Add(new HouseholdMember(household.Id, creatorUserId, creatorDisplayName, HouseholdRole.Owner));
        household.Raise(new HouseholdCreatedEvent(household.Id, creatorUserId));

        return household;
    }

    public bool IsMember(Guid userId) => _members.Any(m => m.UserId == userId);

    public HouseholdInvitation InviteMember(string email, Guid invitedByUserId)
    {
        if (!IsMember(invitedByUserId))
        {
            throw new InvalidOperationException("Only household members can invite new members.");
        }

        var normalizedEmail = email.Trim().ToLowerInvariant();
        if (_invitations.Any(i => i.Email == normalizedEmail && i.IsUsable))
        {
            throw new InvalidOperationException("This email already has a pending invitation.");
        }

        var invitation = new HouseholdInvitation(Id, normalizedEmail, invitedByUserId);
        _invitations.Add(invitation);
        Raise(new MemberInvitedEvent(Id, invitation.Id, invitation.Email, invitedByUserId));

        return invitation;
    }

    public HouseholdMember AcceptInvitation(string token, Guid userId, string displayName)
    {
        var invitation = _invitations.SingleOrDefault(i => i.Token == token)
            ?? throw new InvalidOperationException("Invitation not found.");

        if (!invitation.IsUsable)
        {
            throw new InvalidOperationException("This invitation is no longer valid.");
        }

        if (IsMember(userId))
        {
            throw new InvalidOperationException("You are already a member of this household.");
        }

        invitation.MarkAccepted();
        var member = new HouseholdMember(Id, userId, displayName, HouseholdRole.Member);
        _members.Add(member);
        Raise(new MemberJoinedEvent(Id, userId));

        return member;
    }
}
