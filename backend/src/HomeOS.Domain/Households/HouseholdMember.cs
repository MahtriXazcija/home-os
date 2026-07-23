using HomeOS.Domain.Common;

namespace HomeOS.Domain.Households;

public class HouseholdMember : Entity
{
    public Guid HouseholdId { get; private set; }
    public Guid UserId { get; private set; }
    public string DisplayName { get; private set; } = string.Empty;
    public HouseholdRole Role { get; private set; }
    public DateTime JoinedAtUtc { get; private set; }

    private HouseholdMember() { }

    internal HouseholdMember(Guid householdId, Guid userId, string displayName, HouseholdRole role)
    {
        HouseholdId = householdId;
        UserId = userId;
        DisplayName = displayName;
        Role = role;
        JoinedAtUtc = DateTime.UtcNow;
    }
}
