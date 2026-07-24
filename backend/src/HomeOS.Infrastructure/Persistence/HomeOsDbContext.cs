using HomeOS.Application.Common;
using HomeOS.Domain.Apps;
using HomeOS.Domain.Boards;
using HomeOS.Domain.Calendar;
using HomeOS.Domain.Common;
using HomeOS.Domain.Finance;
using HomeOS.Domain.Households;
using HomeOS.Domain.LifeAdmin;
using HomeOS.Domain.MealPlanner;
using HomeOS.Domain.Notes;
using HomeOS.Domain.Notifications;
using HomeOS.Domain.Reminders;
using HomeOS.Domain.Tasks;
using HomeOS.Infrastructure.Identity;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HomeOS.Infrastructure.Persistence;

public class HomeOsDbContext(DbContextOptions<HomeOsDbContext> options, IPublisher publisher)
    : IdentityDbContext<AppUser, IdentityRole<Guid>, Guid>(options), IAppDbContext
{
    public DbSet<Household> Households => Set<Household>();
    public DbSet<TaskItem> Tasks => Set<TaskItem>();
    public DbSet<Board> Boards => Set<Board>();
    public DbSet<CalendarEvent> CalendarEvents => Set<CalendarEvent>();
    public DbSet<Reminder> Reminders => Set<Reminder>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<NotificationPreference> NotificationPreferences => Set<NotificationPreference>();
    public DbSet<Note> Notes => Set<Note>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<Bill> Bills => Set<Bill>();
    public DbSet<Budget> Budgets => Set<Budget>();
    public DbSet<HouseholdDocument> Documents => Set<HouseholdDocument>();
    public DbSet<Contact> Contacts => Set<Contact>();
    public DbSet<ShoppingItem> ShoppingItems => Set<ShoppingItem>();
    public DbSet<AppInstallation> AppInstallations => Set<AppInstallation>();
    public DbSet<MealPlanEntry> MealPlanEntries => Set<MealPlanEntry>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Household>(entity =>
        {
            entity.ToTable("Households");
            entity.Property(h => h.Name).HasMaxLength(200).IsRequired();

            entity.HasMany(h => h.Members)
                .WithOne()
                .HasForeignKey(m => m.HouseholdId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.Navigation(h => h.Members).UsePropertyAccessMode(PropertyAccessMode.Field);

            entity.HasMany(h => h.Invitations)
                .WithOne()
                .HasForeignKey(i => i.HouseholdId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.Navigation(h => h.Invitations).UsePropertyAccessMode(PropertyAccessMode.Field);
        });

        builder.Entity<HouseholdMember>(entity =>
        {
            entity.ToTable("HouseholdMembers");
            entity.Property(m => m.DisplayName).HasMaxLength(200);
            entity.HasIndex(m => m.UserId).IsUnique();
        });

        builder.Entity<HouseholdInvitation>(entity =>
        {
            entity.ToTable("HouseholdInvitations");
            entity.Property(i => i.Email).HasMaxLength(320);
            entity.HasIndex(i => i.Token).IsUnique();
        });

        builder.Entity<TaskItem>(entity =>
        {
            entity.ToTable("Tasks");
            entity.Property(t => t.Title).HasMaxLength(200).IsRequired();
            entity.HasIndex(t => t.HouseholdId);
            entity.HasIndex(t => t.BoardId);
            entity.HasIndex(t => t.DueDateUtc);
        });

        builder.Entity<Board>(entity =>
        {
            entity.ToTable("Boards");
            entity.Property(b => b.Name).HasMaxLength(200).IsRequired();
            entity.HasIndex(b => b.HouseholdId);
        });

        builder.Entity<CalendarEvent>(entity =>
        {
            entity.ToTable("CalendarEvents");
            entity.Property(e => e.Title).HasMaxLength(200).IsRequired();
            entity.HasIndex(e => e.HouseholdId);
            entity.HasIndex(e => e.StartUtc);
        });

        builder.Entity<Reminder>(entity =>
        {
            entity.ToTable("Reminders");
            entity.Property(r => r.Title).HasMaxLength(200).IsRequired();
            entity.HasIndex(r => r.TargetUserId);
            entity.HasIndex(r => new { r.IsFired, r.RemindAtUtc });
        });

        builder.Entity<Notification>(entity =>
        {
            entity.ToTable("Notifications");
            entity.Property(n => n.Title).HasMaxLength(200).IsRequired();
            entity.HasIndex(n => new { n.UserId, n.IsRead });
        });

        builder.Entity<NotificationPreference>(entity =>
        {
            entity.ToTable("NotificationPreferences");
            entity.HasIndex(p => new { p.UserId, p.Category }).IsUnique();
        });

        builder.Entity<Note>(entity =>
        {
            entity.ToTable("Notes");
            entity.Property(n => n.Title).HasMaxLength(200);
            entity.HasIndex(n => n.HouseholdId);
            entity.HasIndex(n => n.JournalDate);
        });

        builder.Entity<Transaction>(entity =>
        {
            entity.ToTable("Transactions");
            entity.Property(t => t.Category).HasMaxLength(100).IsRequired();
            entity.Property(t => t.Amount).HasPrecision(12, 2);
            entity.HasIndex(t => t.HouseholdId);
            entity.HasIndex(t => t.OccurredAtUtc);
        });

        builder.Entity<Bill>(entity =>
        {
            entity.ToTable("Bills");
            entity.Property(b => b.Title).HasMaxLength(200).IsRequired();
            entity.Property(b => b.Category).HasMaxLength(100);
            entity.Property(b => b.Amount).HasPrecision(12, 2);
            entity.HasIndex(b => b.HouseholdId);
            entity.HasIndex(b => b.DueDateUtc);
        });

        builder.Entity<Budget>(entity =>
        {
            entity.ToTable("Budgets");
            entity.Property(b => b.Category).HasMaxLength(100).IsRequired();
            entity.Property(b => b.MonthlyLimit).HasPrecision(12, 2);
            entity.HasIndex(b => new { b.HouseholdId, b.Category }).IsUnique();
        });

        builder.Entity<HouseholdDocument>(entity =>
        {
            entity.ToTable("Documents");
            entity.Property(d => d.Title).HasMaxLength(200).IsRequired();
            entity.Property(d => d.Category).HasMaxLength(100);
            entity.HasIndex(d => d.HouseholdId);
        });

        builder.Entity<Contact>(entity =>
        {
            entity.ToTable("Contacts");
            entity.Property(c => c.Name).HasMaxLength(200).IsRequired();
            entity.HasIndex(c => c.HouseholdId);
        });

        builder.Entity<ShoppingItem>(entity =>
        {
            entity.ToTable("ShoppingItems");
            entity.Property(s => s.Text).HasMaxLength(300).IsRequired();
            entity.HasIndex(s => s.HouseholdId);
        });

        builder.Entity<AppInstallation>(entity =>
        {
            entity.ToTable("AppInstallations");
            entity.Property(a => a.AppId).HasMaxLength(100).IsRequired();
            entity.HasIndex(a => new { a.HouseholdId, a.AppId }).IsUnique();
        });

        builder.Entity<MealPlanEntry>(entity =>
        {
            entity.ToTable("MealPlanEntries");
            entity.Property(m => m.Title).HasMaxLength(200).IsRequired();
            entity.HasIndex(m => new { m.HouseholdId, m.MealDate });
        });
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var aggregatesWithEvents = ChangeTracker.Entries<AggregateRoot>()
            .Select(e => e.Entity)
            .Where(a => a.DomainEvents.Count != 0)
            .ToList();

        var result = await base.SaveChangesAsync(cancellationToken);

        foreach (var aggregate in aggregatesWithEvents)
        {
            var events = aggregate.DomainEvents.ToList();
            aggregate.ClearDomainEvents();

            foreach (var domainEvent in events)
            {
                var notificationType = typeof(DomainEventNotification<>).MakeGenericType(domainEvent.GetType());
                var notification = (INotification)Activator.CreateInstance(notificationType, domainEvent)!;
                await publisher.Publish(notification, cancellationToken);
            }
        }

        return result;
    }
}
