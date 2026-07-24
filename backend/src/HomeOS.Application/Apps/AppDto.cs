using HomeOS.Domain.Apps;

namespace HomeOS.Application.Apps;

public record AppDto(
    string Id, string Name, string Icon, string Description, bool IsCore,
    List<string> Permissions, List<string> Publishes, List<string> Subscribes,
    string NavLabel, string NavRoute, List<string> CommandPaletteActions, bool SearchProvider,
    bool IsInstalled, List<string> GrantedPermissions)
{
    public static AppDto From(AppManifest manifest, AppInstallation? installation) => new(
        manifest.Id, manifest.Name, manifest.Icon, manifest.Description, manifest.IsCore,
        manifest.Permissions, manifest.Publishes, manifest.Subscribes,
        manifest.Ui.NavLabel, manifest.Ui.NavRoute, manifest.Ui.CommandPaletteActions, manifest.Ui.SearchProvider,
        installation is not null, installation?.GrantedPermissions ?? []);
}
