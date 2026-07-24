namespace HomeOS.Domain.Apps;

/// <summary>
/// Mirrors the manifest shape in docs/app-sdk.md §1. Not a database entity —
/// this is the static description of what an app is and needs; whether a
/// household has actually installed it lives in <see cref="AppInstallation"/>.
/// </summary>
public record AppManifest(
    string Id,
    string Name,
    string Icon,
    string Description,
    bool IsCore,
    List<string> Permissions,
    List<string> Publishes,
    List<string> Subscribes,
    AppUiManifest Ui);

public record AppUiManifest(string NavLabel, string NavRoute, List<string> CommandPaletteActions, bool SearchProvider);
