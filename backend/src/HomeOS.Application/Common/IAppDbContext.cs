using HomeOS.Domain.Boards;
using HomeOS.Domain.Calendar;
using HomeOS.Domain.Tasks;
using Microsoft.EntityFrameworkCore;

namespace HomeOS.Application.Common;

/// <summary>
/// Flat entities without aggregate-internal child collections (Task, Board,
/// CalendarEvent) don't hit the client-generated-key tracking gotcha that
/// Household's child entities did (see IHouseholdRepository.TrackNew), so
/// they're exposed directly rather than wrapped in a per-entity repository.
/// </summary>
public interface IAppDbContext
{
    DbSet<TaskItem> Tasks { get; }
    DbSet<Board> Boards { get; }
    DbSet<CalendarEvent> CalendarEvents { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
