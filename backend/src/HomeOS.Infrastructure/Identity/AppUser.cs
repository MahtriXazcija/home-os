using Microsoft.AspNetCore.Identity;

namespace HomeOS.Infrastructure.Identity;

public class AppUser : IdentityUser<Guid>
{
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// A small base64 data: URL for the avatar (client resizes to a thumbnail
    /// before upload — see ProfileController.MaxPhotoLength). No blob storage
    /// is wired up for this project, so it lives directly on the user row.
    /// </summary>
    public string? PhotoDataUrl { get; set; }
}
