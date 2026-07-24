using HomeOS.Domain.Common;

namespace HomeOS.Domain.Apps;

/// <summary>
/// A household's install record for one app from the <see cref="AppCatalog"/>
/// — which permissions were actually granted, reviewable and revocable
/// (docs/app-sdk.md §4 "The household stays in control").
/// </summary>
public class AppInstallation : Entity
{
    public Guid HouseholdId { get; private set; }
    public string AppId { get; private set; } = string.Empty;
    public List<string> GrantedPermissions { get; private set; } = [];
    public DateTime InstalledAtUtc { get; private set; }

    private AppInstallation() { }

    public static AppInstallation Create(Guid householdId, string appId, List<string> grantedPermissions)
    {
        if (string.IsNullOrWhiteSpace(appId))
        {
            throw new ArgumentException("App id is required.", nameof(appId));
        }

        return new AppInstallation
        {
            HouseholdId = householdId,
            AppId = appId,
            GrantedPermissions = grantedPermissions,
            InstalledAtUtc = DateTime.UtcNow
        };
    }
}
