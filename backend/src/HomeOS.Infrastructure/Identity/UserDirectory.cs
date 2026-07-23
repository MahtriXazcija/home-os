using HomeOS.Application.Common;
using Microsoft.AspNetCore.Identity;

namespace HomeOS.Infrastructure.Identity;

public class UserDirectory(UserManager<AppUser> userManager) : IUserDirectory
{
    public async Task<UserContact?> GetContactAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        return user is null ? null : new UserContact(user.Id, user.Email!, user.DisplayName);
    }
}
