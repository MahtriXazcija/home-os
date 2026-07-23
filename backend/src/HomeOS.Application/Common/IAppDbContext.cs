using HomeOS.Domain.Boards;
using HomeOS.Domain.Calendar;
using HomeOS.Domain.Finance;
using HomeOS.Domain.LifeAdmin;
using HomeOS.Domain.Notes;
using HomeOS.Domain.Notifications;
using HomeOS.Domain.Reminders;
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
    DbSet<Reminder> Reminders { get; }
    DbSet<Notification> Notifications { get; }
    DbSet<NotificationPreference> NotificationPreferences { get; }
    DbSet<Note> Notes { get; }
    DbSet<Transaction> Transactions { get; }
    DbSet<Bill> Bills { get; }
    DbSet<Budget> Budgets { get; }
    DbSet<HouseholdDocument> Documents { get; }
    DbSet<Contact> Contacts { get; }
    DbSet<ShoppingItem> ShoppingItems { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
