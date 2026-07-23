using HomeOS.Domain.Common;

namespace HomeOS.Domain.Tasks;

public class TaskItem : AggregateRoot
{
    public Guid HouseholdId { get; private set; }
    public Guid? BoardId { get; private set; }
    public Guid? ParentTaskId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public DateTime? DueDateUtc { get; private set; }
    public TaskPriority Priority { get; private set; }
    public HomeTaskStatus Status { get; private set; }
    public Guid? AssignedToUserId { get; private set; }
    public RecurrenceRule Recurrence { get; private set; }
    public List<string> Tags { get; private set; } = [];
    public bool IsCompleted { get; private set; }
    public DateTime? CompletedAtUtc { get; private set; }
    public Guid CreatedByUserId { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    private TaskItem() { }

    public static TaskItem Create(
        Guid householdId,
        string title,
        Guid createdByUserId,
        string? description = null,
        DateTime? dueDateUtc = null,
        TaskPriority priority = TaskPriority.Medium,
        Guid? assignedToUserId = null,
        Guid? boardId = null,
        Guid? parentTaskId = null,
        RecurrenceRule recurrence = RecurrenceRule.None,
        List<string>? tags = null)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("Task title is required.", nameof(title));
        }

        var task = new TaskItem
        {
            HouseholdId = householdId,
            Title = title.Trim(),
            Description = description,
            DueDateUtc = dueDateUtc,
            Priority = priority,
            Status = HomeTaskStatus.Todo,
            AssignedToUserId = assignedToUserId,
            BoardId = boardId,
            ParentTaskId = parentTaskId,
            Recurrence = recurrence,
            Tags = tags ?? [],
            CreatedByUserId = createdByUserId,
            CreatedAtUtc = DateTime.UtcNow
        };

        task.Raise(new TaskCreatedEvent(task.Id, householdId, task.Title, dueDateUtc, assignedToUserId));
        return task;
    }

    public void UpdateDetails(string title, string? description, DateTime? dueDateUtc, TaskPriority priority, Guid? assignedToUserId, RecurrenceRule recurrence, List<string> tags)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("Task title is required.", nameof(title));
        }

        Title = title.Trim();
        Description = description;
        DueDateUtc = dueDateUtc;
        Priority = priority;
        AssignedToUserId = assignedToUserId;
        Recurrence = recurrence;
        Tags = tags;
    }

    public void ChangeStatus(HomeTaskStatus newStatus)
    {
        if (Status == newStatus) return;
        var from = Status;
        Status = newStatus;
        if (newStatus == HomeTaskStatus.Done && !IsCompleted)
        {
            IsCompleted = true;
            CompletedAtUtc = DateTime.UtcNow;
        }
        else if (newStatus != HomeTaskStatus.Done && IsCompleted)
        {
            IsCompleted = false;
            CompletedAtUtc = null;
        }
        Raise(new BoardCardMovedEvent(Id, HouseholdId, from, newStatus));
    }

    public void Complete(Guid completedByUserId)
    {
        IsCompleted = true;
        CompletedAtUtc = DateTime.UtcNow;
        Status = HomeTaskStatus.Done;
        Raise(new TaskCompletedEvent(Id, HouseholdId, completedByUserId));
    }

    public void Reopen()
    {
        IsCompleted = false;
        CompletedAtUtc = null;
        if (Status == HomeTaskStatus.Done)
        {
            Status = HomeTaskStatus.Doing;
        }
    }

    public void MoveToBoard(Guid? boardId) => BoardId = boardId;

    /// <summary>Used by the recurrence handler to compute the next instance's due date.</summary>
    public DateTime? NextDueDate() => Recurrence switch
    {
        RecurrenceRule.Daily => DueDateUtc?.AddDays(1),
        RecurrenceRule.Weekly => DueDateUtc?.AddDays(7),
        RecurrenceRule.Monthly => DueDateUtc?.AddMonths(1),
        _ => null
    };
}
