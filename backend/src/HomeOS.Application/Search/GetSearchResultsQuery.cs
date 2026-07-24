using HomeOS.Application.Common;
using HomeOS.Domain.Apps;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HomeOS.Application.Search;

public record GetSearchResultsQuery(Guid HouseholdId, string Query) : IRequest<List<SearchResultDto>>;

/// <summary>
/// Only searches apps the household actually has installed — the one place
/// in Phase 5 where "installed" has a real runtime effect beyond hiding a
/// nav link, matching "a new app only reaches what the household agreed to
/// give it" (docs/app-sdk.md §4).
/// </summary>
public class GetSearchResultsQueryHandler(IAppDbContext db) : IRequestHandler<GetSearchResultsQuery, List<SearchResultDto>>
{
    public async Task<List<SearchResultDto>> Handle(GetSearchResultsQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Query) || request.Query.Length < 2)
        {
            return [];
        }

        var installedAppIds = await db.AppInstallations
            .Where(a => a.HouseholdId == request.HouseholdId)
            .Select(a => a.AppId)
            .ToListAsync(cancellationToken);

        var searchableInstalledIds = AppCatalog.All
            .Where(m => m.Ui.SearchProvider && installedAppIds.Contains(m.Id))
            .Select(m => m.Id)
            .ToHashSet();

        var needle = request.Query.ToLower();
        var results = new List<SearchResultDto>();

        if (searchableInstalledIds.Contains("tasks"))
        {
            var tasks = await db.Tasks
                .Where(t => t.HouseholdId == request.HouseholdId && t.Title.ToLower().Contains(needle))
                .Take(10)
                .ToListAsync(cancellationToken);
            results.AddRange(tasks.Select(t => new SearchResultDto("tasks", "task", t.Id, t.Title, "/tasks")));
        }

        if (searchableInstalledIds.Contains("notes"))
        {
            var notes = await db.Notes
                .Where(n => n.HouseholdId == request.HouseholdId && (n.Content.ToLower().Contains(needle) || (n.Title != null && n.Title.ToLower().Contains(needle))))
                .Take(10)
                .ToListAsync(cancellationToken);
            results.AddRange(notes.Select(n => new SearchResultDto("notes", "note", n.Id, n.Title ?? n.Content[..Math.Min(40, n.Content.Length)], "/notes")));
        }

        if (searchableInstalledIds.Contains("life-admin"))
        {
            var contacts = await db.Contacts
                .Where(c => c.HouseholdId == request.HouseholdId && c.Name.ToLower().Contains(needle))
                .Take(10)
                .ToListAsync(cancellationToken);
            results.AddRange(contacts.Select(c => new SearchResultDto("life-admin", "contact", c.Id, c.Name, "/life-admin")));

            var documents = await db.Documents
                .Where(d => d.HouseholdId == request.HouseholdId && d.Title.ToLower().Contains(needle))
                .Take(10)
                .ToListAsync(cancellationToken);
            results.AddRange(documents.Select(d => new SearchResultDto("life-admin", "document", d.Id, d.Title, "/life-admin")));
        }

        if (searchableInstalledIds.Contains("meal-planner"))
        {
            var meals = await db.MealPlanEntries
                .Where(m => m.HouseholdId == request.HouseholdId && m.Title.ToLower().Contains(needle))
                .Take(10)
                .ToListAsync(cancellationToken);
            results.AddRange(meals.Select(m => new SearchResultDto("meal-planner", "meal", m.Id, m.Title, "/meal-planner")));
        }

        return results;
    }
}
