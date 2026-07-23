using HomeOS.Domain.Tasks;

namespace HomeOS.Application.Tasks;

public record TaskDto(
    Guid Id,
    Guid HouseholdId,
    Guid? BoardId,
    Guid? ParentTaskId,
    string Title,
    string? Description,
    DateTime? DueDateUtc,
    string Priority,
    string Status,
    Guid? AssignedToUserId,
    string Recurrence,
    List<string> Tags,
    bool IsCompleted,
    DateTime? CompletedAtUtc,
    Guid CreatedByUserId,
    DateTime CreatedAtUtc)
{
    public static TaskDto From(TaskItem task) => new(
        task.Id,
        task.HouseholdId,
        task.BoardId,
        task.ParentTaskId,
        task.Title,
        task.Description,
        task.DueDateUtc,
        task.Priority.ToString(),
        task.Status.ToString(),
        task.AssignedToUserId,
        task.Recurrence.ToString(),
        task.Tags,
        task.IsCompleted,
        task.CompletedAtUtc,
        task.CreatedByUserId,
        task.CreatedAtUtc);
}
