using HomeOS.Domain.Calendar;

namespace HomeOS.Application.Calendar;

public record CalendarEventDto(Guid Id, Guid HouseholdId, string Title, string? Description, DateTime StartUtc, DateTime EndUtc, bool IsAllDay, Guid CreatedByUserId)
{
    public static CalendarEventDto From(CalendarEvent evt) => new(
        evt.Id, evt.HouseholdId, evt.Title, evt.Description, evt.StartUtc, evt.EndUtc, evt.IsAllDay, evt.CreatedByUserId);
}

/// <summary>
/// A calendar shows CalendarEvents and Tasks-with-due-dates side by side —
/// the "a task with a due date appears on the calendar automatically" rule
/// from the task brief, done at query time rather than by duplicating data.
/// </summary>
public record CalendarItemDto(Guid Id, string Kind, string Title, DateTime StartUtc, DateTime EndUtc, bool IsAllDay);
