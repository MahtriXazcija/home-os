using HomeOS.Domain.Common;

namespace HomeOS.Domain.Calendar;

public class CalendarEvent : AggregateRoot
{
    public Guid HouseholdId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public DateTime StartUtc { get; private set; }
    public DateTime EndUtc { get; private set; }
    public bool IsAllDay { get; private set; }
    public Guid CreatedByUserId { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    private CalendarEvent() { }

    public static CalendarEvent Create(Guid householdId, string title, DateTime startUtc, DateTime endUtc, bool isAllDay, Guid createdByUserId, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("Event title is required.", nameof(title));
        }
        if (endUtc < startUtc)
        {
            throw new ArgumentException("Event end must not be before its start.", nameof(endUtc));
        }

        var evt = new CalendarEvent
        {
            HouseholdId = householdId,
            Title = title.Trim(),
            Description = description,
            StartUtc = startUtc,
            EndUtc = endUtc,
            IsAllDay = isAllDay,
            CreatedByUserId = createdByUserId,
            CreatedAtUtc = DateTime.UtcNow
        };

        evt.Raise(new CalendarEventCreatedEvent(evt.Id, householdId, startUtc, endUtc));
        return evt;
    }
}
