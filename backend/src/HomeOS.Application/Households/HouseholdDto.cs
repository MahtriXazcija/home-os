using HomeOS.Domain.Households;

namespace HomeOS.Application.Households;

public record HouseholdDto(Guid Id, string Name, DateTime CreatedAtUtc, IReadOnlyList<MemberDto> Members)
{
    public static HouseholdDto From(Household household) => new(
        household.Id,
        household.Name,
        household.CreatedAtUtc,
        household.Members.Select(MemberDto.From).ToList());
}

public record MemberDto(Guid UserId, string DisplayName, string Role, DateTime JoinedAtUtc)
{
    public static MemberDto From(HouseholdMember member) => new(
        member.UserId,
        member.DisplayName,
        member.Role.ToString(),
        member.JoinedAtUtc);
}

public record InvitationDto(Guid Id, string Email, string Token, DateTime ExpiresAtUtc)
{
    public static InvitationDto From(HouseholdInvitation invitation) => new(
        invitation.Id,
        invitation.Email,
        invitation.Token,
        invitation.ExpiresAtUtc);
}
