using HomeOS.Application.Households;
using HomeOS.Domain.Households;
using Microsoft.EntityFrameworkCore;

namespace HomeOS.Infrastructure.Persistence;

public class HouseholdRepository(HomeOsDbContext db) : IHouseholdRepository
{
    private IQueryable<Household> WithChildren() => db.Households
        .Include(h => h.Members)
        .Include(h => h.Invitations);

    public Task<Household?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        WithChildren().FirstOrDefaultAsync(h => h.Id == id, cancellationToken);

    public Task<Household?> GetByMemberUserIdAsync(Guid userId, CancellationToken cancellationToken) =>
        WithChildren().FirstOrDefaultAsync(h => h.Members.Any(m => m.UserId == userId), cancellationToken);

    public Task<Household?> GetByInvitationTokenAsync(string token, CancellationToken cancellationToken) =>
        WithChildren().FirstOrDefaultAsync(h => h.Invitations.Any(i => i.Token == token), cancellationToken);

    public void Add(Household household) => db.Households.Add(household);

    public Task SaveChangesAsync(CancellationToken cancellationToken) => db.SaveChangesAsync(cancellationToken);
}
