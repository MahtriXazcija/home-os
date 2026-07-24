namespace HomeOS.Application.Assistant;

public record PageAssistantInfo(string Title, string Description);

/// <summary>
/// What the in-app page assistant is allowed to know about each page —
/// static and accurate rather than inferred, so it can't hallucinate a
/// feature that doesn't exist. Keyed by the frontend route segment.
/// </summary>
public static class PageAssistantContext
{
    private static readonly Dictionary<string, PageAssistantInfo> Pages = new()
    {
        ["dashboard"] = new("Dashboard",
            "The Today screen — pulls together tasks due, today's calendar events, upcoming bills, and active reminders from every other module into one glance."),
        ["tasks"] = new("Tasks",
            "Create and manage tasks: title, due date, priority (Low/Medium/High), assignee, recurrence, and tags. Tasks completed here also show up on Kanban and Calendar."),
        ["kanban"] = new("Kanban",
            "A board view of tasks organized into To do / Doing / Done columns. Cards are the same tasks as the Tasks page — changing a card's status here updates it everywhere. Cards can have a due date, priority, and assignee set when created, and the column is changed with a status dropdown on each card (no drag-and-drop)."),
        ["calendar"] = new("Calendar",
            "Month, week, and day views of events plus task due dates, switched with a Month/Week/Day toggle. Events can be added directly; task due dates appear automatically."),
        ["reminders"] = new("Reminders",
            "One-off or recurring reminders aimed at any household member. Fires in-app and by email (if the member opted in) at the scheduled time. Grouped into Overdue/Today/This week/Later."),
        ["notes"] = new("Notes",
            "Quick notes with optional tags, plus a daily journal (one entry per day, spotlighted at the top of the page)."),
        ["finance"] = new("Finance",
            "Income/expense transactions by category, monthly budgets per category, and recurring bills. When a bill's due date arrives, it automatically creates a task and a reminder."),
        ["life-admin"] = new("Life Admin",
            "Household documents with renewal dates, emergency/important contacts, and a shared shopping list."),
        ["meal-planner"] = new("Meal Planner",
            "Plan meals for the week on a 7-day grid; a planned meal can optionally create a shopping task."),
        ["chat"] = new("Chat",
            "A simple group chat visible only to members of this household, with Delivered/Seen-by receipts and an unread badge in the sidebar."),
        ["apps"] = new("Manage apps",
            "The catalog of every module (built-in or third-party) — install or uninstall them, and see exactly which permissions each one requests."),
        ["profile"] = new("Profile",
            "Your personal account details: display name, phone number, and profile photo. Also shows your role (Administrator or Member) in this household."),
    };

    public static PageAssistantInfo Describe(string pageId) =>
        Pages.TryGetValue(pageId, out var info) ? info : new(pageId, "A module in Home OS.");
}
