using HomeOS.Domain.Households;

namespace HomeOS.Application.Households;

public interface IHouseholdRepository
{
    Task<Household?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<Household?> GetByMemberUserIdAsync(Guid userId, CancellationToken cancellationToken);
    Task<Household?> GetByInvitationTokenAsync(string token, CancellationToken cancellationToken);
    void Add(Household household);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
