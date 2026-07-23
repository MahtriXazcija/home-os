using HomeOS.Application.Common;
using HomeOS.Domain.Common;
using HomeOS.Domain.Households;
using HomeOS.Infrastructure.Identity;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HomeOS.Infrastructure.Persistence;

public class HomeOsDbContext(DbContextOptions<HomeOsDbContext> options, IPublisher publisher)
    : IdentityDbContext<AppUser, IdentityRole<Guid>, Guid>(options)
{
    public DbSet<Household> Households => Set<Household>();

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
