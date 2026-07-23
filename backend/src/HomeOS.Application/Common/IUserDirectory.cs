namespace HomeOS.Application.Common;

public record UserContact(Guid UserId, string Email, string DisplayName);

public interface IUserDirectory
{
    Task<UserContact?> GetContactAsync(Guid userId, CancellationToken cancellationToken);
}
