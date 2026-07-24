namespace HomeOS.Domain.Apps;

/// <summary>
/// The 8 built-in apps, described the exact same way a third-party app
/// would be (see docs/app-sdk.md §1) — "installs the same way the built-in
/// ones do, with no special-casing" only holds if the built-ins actually go
/// through this list too, so they do. Dashboard and Tasks are marked core
/// (always installed, can't be removed) because Kanban/Calendar/Finance all
/// assume Tasks exists; everything else is a real install/uninstall toggle.
/// </summary>
public static class AppCatalog
{
    public static readonly IReadOnlyList<AppManifest> All =
    [
        new("dashboard", "Dashboard", "home", "The Today screen — pulls together what's due, happening, or coming up.",
            true, ["tasks:read", "calendar:read", "reminders:read", "finance:read"], [], [],
            new AppUiManifest("Dashboard", "/", [], false)),

        new("tasks", "Tasks", "check-square", "Create and manage tasks with due dates, priority, and assignees.",
            true, ["tasks:read", "tasks:write", "members:read"], ["task.created", "task.completed"], [],
            new AppUiManifest("Tasks", "/tasks", ["add-task"], true)),

        new("kanban", "Kanban", "columns", "Board view of tasks organized into columns.",
            false, ["tasks:read", "tasks:write"], ["board.card.moved"], [],
            new AppUiManifest("Kanban", "/kanban", ["add-board"], false)),

        new("calendar", "Calendar", "calendar", "Month/week/day views; tasks with due dates show up automatically.",
            false, ["calendar:read", "calendar:write", "tasks:read"], ["calendarEvent.created"], ["task.created"],
            new AppUiManifest("Calendar", "/calendar", ["add-event"], false)),

        new("reminders", "Reminders", "bell", "One-off or recurring reminders aimed at any member.",
            false, ["reminders:create", "notifications:send", "members:read"], ["reminder.fired"], [],
            new AppUiManifest("Reminders", "/reminders", ["add-reminder"], false)),

        new("notes", "Notes", "file-text", "Quick notes with tags and a daily journal.",
            false, ["notes:write"], ["note.created"], [],
            new AppUiManifest("Notes", "/notes", ["add-note"], true)),

        new("finance", "Finance", "dollar-sign", "Income/expenses by category, budgets, and recurring bills.",
            false, ["finance:write", "tasks:create", "reminders:create"], ["bill.created", "bill.dueSoon"], [],
            new AppUiManifest("Finance", "/finance", ["add-bill", "add-transaction"], false)),

        new("life-admin", "Life Admin", "archive", "Household documents, contacts, and the shared shopping list.",
            false, ["documents:write", "contacts:write", "reminders:create"], ["document.renewalDue"], [],
            new AppUiManifest("Life Admin", "/life-admin", ["add-document", "add-shopping-item"], true)),
    ];

    public static AppManifest? Find(string id) => All.FirstOrDefault(a => a.Id == id);
}
