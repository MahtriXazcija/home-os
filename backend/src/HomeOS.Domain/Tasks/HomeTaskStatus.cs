namespace HomeOS.Domain.Tasks;

/// <summary>Named to avoid colliding with System.Threading.Tasks.TaskStatus — this is the Kanban column.</summary>
public enum HomeTaskStatus
{
    Todo,
    Doing,
    Done
}
