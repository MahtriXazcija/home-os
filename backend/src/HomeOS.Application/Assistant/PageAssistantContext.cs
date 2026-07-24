namespace HomeOS.Application.Assistant;

public record PageFaqEntry(string[] Keywords, string Answer);

public record PageAssistantInfo(string Title, string Brief, string Detailed, List<PageFaqEntry> Faq);

/// <summary>
/// Everything the in-app page assistant can say — written once, per page,
/// instead of delegated to an external LLM. Guarantees it can only ever
/// talk about Home OS pages (there's nothing else for it to say), costs
/// nothing to run, and never depends on a third party's API quota.
/// </summary>
public static class PageAssistantContext
{
    private static readonly Dictionary<string, PageAssistantInfo> Pages = new()
    {
        ["dashboard"] = new(
            "Dashboard",
            "This is Today — a live summary of tasks due, today's events, upcoming bills, and active reminders pulled from every other module.",
            "The Dashboard is the Today screen: four cards show tasks due (overdue first, then upcoming), today's calendar events, bills coming due, and your active reminders. Everything here is read-only and pulled live from Tasks, Calendar, Finance, and Reminders — you don't create anything on this page itself, just use the linked pages and it appears here automatically. The status strip at the bottom shows whether the app is connected to the backend.",
            [
                new(["empty", "nothing", "no data", "blank"], "An empty card just means there's nothing due in that category right now — add a task with a due date, an event, or a bill on their own pages and it'll show up here."),
                new(["add", "create", "new"], "The Dashboard itself doesn't create anything — use Tasks, Calendar, Finance, or Reminders to add items; they appear here automatically."),
            ]),

        ["tasks"] = new(
            "Tasks",
            "Create and manage tasks with a due date, priority, and an assignee — everything that needs doing.",
            "Tasks is the master list everything else builds on. The form at the top lets you set a title, due date, priority (Low/Medium/High), assignee (any household member), recurrence (One-off/Daily/Weekly/Monthly), and comma-separated tags. Each row has a checkbox to mark it complete (click again to reopen), and a Delete link. The list sorts by due date with completed tasks sinking to the bottom, and overdue tasks are highlighted. These same tasks also show up on Kanban and Calendar — they're one underlying record, not separate copies.",
            [
                new(["priority"], "Priority is Low/Medium/High, shown as a colored pill — set it in the dropdown when creating or editing a task."),
                new(["assign", "who", "member"], "Pick anyone from your household in the assignee dropdown when creating a task; leave it on 'Unassigned' to skip it."),
                new(["recur", "repeat", "daily", "weekly", "monthly"], "Set recurrence to Daily/Weekly/Monthly in the dropdown, or leave it as 'Doesn't repeat'."),
                new(["delete", "remove"], "Click 'Delete' on the right side of any task row."),
                new(["complete", "done", "check", "finish"], "Tick the checkbox on the left of a task to mark it complete — click it again to reopen the task."),
                new(["tag"], "Add comma-separated tags in the tags field when creating a task; they show as small pills on the row."),
            ]),

        ["kanban"] = new(
            "Kanban",
            "A board view of the same tasks, organized into To do / Doing / Done columns.",
            "Kanban shows your tasks as cards across three columns: To do, Doing, Done. Pick a board from the tabs at the top (or create a new one). The quick-add form lets you set a due date, priority, and assignee when creating a card. To move a card between columns, use the small status dropdown at the bottom of the card — there's no drag-and-drop, it's deliberately a plain dropdown so it's reliable on any device. These cards are the exact same tasks as the Tasks page, just viewed differently — changing status here updates it everywhere.",
            [
                new(["move", "drag", "column", "status"], "Cards don't need dragging — use the small status dropdown at the bottom of each card to move it between To do / Doing / Done."),
                new(["board", "new board"], "Type a name in 'New board name' next to the board tabs and click '+ Board'."),
                new(["priority", "due date", "assign"], "Set those in the quick-add form when creating a card — same fields as the Tasks page."),
            ]),

        ["calendar"] = new(
            "Calendar",
            "Month, week, and day views of your events plus task due dates in one place.",
            "Calendar has three views switched with the Month/Week/Day buttons in the toolbar. Month shows a classic 6-week grid; Week shows a 7-day agenda; Day shows a single-day timeline. Prev/Next steps by whichever view is active — a month, a week, or a day at a time. Add an event with the title+date fields in the toolbar. Tasks with a due date appear automatically as a differently-colored item on their due day — you don't add those manually here, they come from the Tasks page.",
            [
                new(["view", "week", "day", "month", "switch"], "Use the Month / Week / Day buttons at the top left of the toolbar to switch views."),
                new(["task", "due date"], "Tasks with a due date show up automatically as a separate colored item on their due day — add them from the Tasks page, not here."),
                new(["add event", "new event"], "Type a title, pick a date, and click 'Add event' in the toolbar."),
            ]),

        ["reminders"] = new(
            "Reminders",
            "One-off or recurring reminders aimed at any household member, with email as well as in-app.",
            "Reminders can target yourself or any other household member, fire once or repeat (Daily/Weekly/Monthly), and optionally carry a note. When one fires it creates both an in-app notification and — if the target member has that category enabled in their notification settings — an email. The list is grouped into Overdue, Today, This week, and Later so the closest ones are easy to find. Click Cancel to remove one before it fires.",
            [
                new(["email", "notify", "notification"], "Reminders fire both in-app and by email — email only sends if the target member has that category enabled (bell icon → Settings)."),
                new(["recur", "repeat", "daily", "weekly", "monthly"], "Pick Daily/Weekly/Monthly recurrence when creating the reminder, or leave it as 'One-off'."),
                new(["cancel", "delete", "remove"], "Click 'Cancel' on the right of a reminder row."),
                new(["group", "overdue", "today", "later"], "Reminders are grouped by when they're due: Overdue, Today, This week, Later."),
            ]),

        ["notes"] = new(
            "Notes",
            "Quick notes with tags, plus a daily journal spotlighted at the top.",
            "Notes are simple: a title (optional), content, and comma-separated tags. Tick 'Today's journal' when writing one to make it today's journal entry — Home OS allows one journal entry per day and spotlights it in a highlighted card at the top of the page. Use the 'Journal entries only' checkbox to filter the grid down to just journal entries. Delete any note from the top of its card.",
            [
                new(["journal"], "Check 'Today's journal' when writing a note to make it today's entry — it's spotlighted at the top of the page."),
                new(["tag"], "Add comma-separated tags in the tags field; they show as small pills on the note card."),
                new(["delete", "remove"], "Click 'Delete' at the top of any note card."),
            ]),

        ["finance"] = new(
            "Finance",
            "Income/expense transactions by category, monthly budgets, and recurring bills.",
            "Finance tracks three things: transactions (income or expense, by category and date), monthly budgets per category (to see how much you've spent against a limit), and recurring bills. The one automatic behavior worth knowing: when a bill's due date arrives, Home OS automatically creates a task and a reminder for it — you don't have to remember to pay it manually, it'll show up on Tasks and Reminders on its own.",
            [
                new(["bill"], "When a bill's due date arrives, Finance automatically creates a task and a reminder for it — you don't need to do that yourself."),
                new(["budget"], "Set a monthly limit per category; the page shows how much you've spent against it so far."),
                new(["transaction", "expense", "income"], "Log both income and expenses with a category, amount, and date."),
            ]),

        ["life-admin"] = new(
            "Life Admin",
            "Household documents with renewal dates, important contacts, and a shared shopping list.",
            "Life Admin has three independent sections, each with its own quick-add form: Documents (with a category and optional renewal date — the stat strip at the top counts how many renew within 30 days), Contacts (name and phone), and a shared Shopping list (check items off as you buy them). Nothing here connects automatically to other modules — it's a straightforward household reference.",
            [
                new(["renew", "expire", "expiring"], "Set a renewal date when adding a document — the stat strip at the top shows how many are renewing within 30 days."),
                new(["shopping", "buy"], "Add items in the Shopping list section; check them off as you buy them."),
                new(["contact", "phone"], "Add a name and phone number in the Contacts section."),
            ]),

        ["meal-planner"] = new(
            "Meal Planner",
            "Plan meals for the week on a 7-day grid, with an optional shopping task.",
            "Meal Planner shows a 7-day grid split into Breakfast/Lunch/Dinner. Use Prev/Next week to navigate. When adding a meal, tick 'Add shopping task' and it'll create a task on the Tasks page for the ingredients — this app doesn't have its own task system, it reuses the one that already exists.",
            [
                new(["shopping task", "grocery", "ingredient"], "Tick 'Add shopping task' when planning a meal and it creates a task on the Tasks page automatically."),
                new(["week", "next week", "prev"], "Use the Prev/Next week buttons above the grid to navigate."),
            ]),

        ["chat"] = new(
            "Chat",
            "A simple group chat visible only to members of this household, with delivered/seen receipts.",
            "Chat is a household-wide group conversation — only your members can see it. It refreshes automatically every few seconds. Your own messages show a 'Delivered' status until another member opens Chat after you sent it, at which point it switches to 'Seen by <name>'. The unread-count badge next to Chat in the sidebar clears automatically the moment you open the page.",
            [
                new(["seen", "delivered", "read", "receipt"], "Your own messages show 'Delivered' until another member opens Chat after your message was sent, then it becomes 'Seen by <name>'."),
                new(["unread", "badge"], "The number badge next to Chat in the sidebar clears automatically once you open the Chat page."),
            ]),

        ["apps"] = new(
            "Manage apps",
            "The catalog of every module — install or uninstall them, and see exactly what each one can access.",
            "Manage apps lists every module on the platform, built-in or third-party, the same way. Each card shows its name, description, and the exact permissions it requests as small tags. Click Install to add it to your sidebar immediately, or Uninstall to remove it (your data from that app is kept, only access is removed). Dashboard and Tasks are marked Core and can't be uninstalled since the rest of the platform depends on them.",
            [
                new(["install"], "Click 'Install' on any app card — it appears in the sidebar immediately."),
                new(["uninstall", "remove app"], "Click 'Uninstall' on a non-core app card — its existing data stays, only the module's access is removed."),
                new(["permission"], "The small tags under each app's description are exactly the permissions it requests — nothing more is granted."),
                new(["core", "can't remove", "cant remove"], "Dashboard and Tasks are core apps — the rest of the platform depends on them, so they can't be uninstalled."),
            ]),

        ["profile"] = new(
            "Profile",
            "Your personal account details: display name, phone number, and photo.",
            "Profile holds your display name, phone number, and a profile photo (click your avatar circle to choose an image — it's automatically resized before saving). Email is shown but read-only since it's tied to how you log in. Your role in this household — Administrator or Member — is shown as a badge; Administrator is whoever created the household.",
            [
                new(["photo", "picture", "avatar", "image"], "Click your avatar circle at the top of the form to choose an image — it's automatically resized before saving."),
                new(["email"], "Email is read-only here since it's tied to your login — there's currently no way to change it in the app."),
                new(["role", "admin", "administrator"], "Your role (Administrator or Member) is shown next to your name — Administrator is whoever created the household."),
            ]),
    };

    public static PageAssistantInfo Describe(string pageId) =>
        Pages.TryGetValue(pageId, out var info)
            ? info
            : new(pageId, "A module in Home OS.", "This page doesn't have detailed help written yet.", []);
}
