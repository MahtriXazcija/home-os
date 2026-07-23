using HomeOS.Domain.Households;

namespace HomeOS.Application.Households;

public interface IHouseholdRepository
{
    Task<Household?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<Household?> GetByMemberUserIdAsync(Guid userId, CancellationToken cancellationToken);
    Task<Household?> GetByInvitationTokenAsync(string token, CancellationToken cancellationToken);
    void Add(Household household);

    /// <summary>
    /// Marks a child entity created mid-lifetime on an already-tracked
    /// aggregate (e.g. a new invitation on an existing household) as newly
    /// inserted. Without this, EF Core's default "non-default key ⇒ already
    /// exists" heuristic treats a fresh Guid-keyed entity discovered via
    /// navigation as an update to a row that isn't there, throwing
    /// DbUpdateConcurrencyException. Entities reached via <see cref="Add"/>
    /// on a brand-new aggregate don't need this — Add() already cascades
    /// Added state to the whole graph.
    /// </summary>
    void TrackNew(object entity);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
