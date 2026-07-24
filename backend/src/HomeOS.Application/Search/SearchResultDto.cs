namespace HomeOS.Application.Search;

/// <summary>
/// One row per hit. AppId + Route tie a result back to the app that owns
/// it — the search surface itself has no idea what a "task" or "note" is,
/// it just merges what each installed searchProvider app returns
/// (docs/app-sdk.md §1 "in search").
/// </summary>
public record SearchResultDto(string AppId, string EntityType, Guid Id, string Title, string Route);
