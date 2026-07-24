using System.Security.Claims;
using System.Text.RegularExpressions;
using HomeOS.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace HomeOS.Api.Controllers;

public record ProfileResponse(Guid UserId, string Email, string DisplayName, string? PhoneNumber, string? PhotoDataUrl);
public record UpdateProfileRequest(string DisplayName, string? PhoneNumber, string? PhotoDataUrl);

[ApiController]
[Route("api/profile")]
[Authorize]
public partial class ProfileController(UserManager<AppUser> userManager) : ControllerBase
{
    // Data URLs come from a client-side-resized JPEG thumbnail (see
    // frontend Profile page) — capped well above what that ever produces,
    // just to keep an arbitrary huge upload out of the database.
    private const int MaxPhotoDataUrlLength = 400_000;

    [GeneratedRegex(@"^\+?[0-9\s\-()]{6,20}$")]
    private static partial Regex PhoneRegex();

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);

    [HttpGet]
    public async Task<ActionResult<ProfileResponse>> Get()
    {
        var user = await userManager.FindByIdAsync(CurrentUserId.ToString());
        if (user is null) return Unauthorized();

        return Ok(new ProfileResponse(user.Id, user.Email!, user.DisplayName, user.PhoneNumber, user.PhotoDataUrl));
    }

    [HttpPut]
    public async Task<ActionResult<ProfileResponse>> Update(UpdateProfileRequest request)
    {
        var user = await userManager.FindByIdAsync(CurrentUserId.ToString());
        if (user is null) return Unauthorized();

        var displayName = request.DisplayName?.Trim() ?? "";
        if (displayName.Length is < 2 or > 100)
        {
            return BadRequest("Display name must be between 2 and 100 characters.");
        }

        var phone = string.IsNullOrWhiteSpace(request.PhoneNumber) ? null : request.PhoneNumber.Trim();
        if (phone is not null && !PhoneRegex().IsMatch(phone))
        {
            return BadRequest("Phone number looks invalid — use digits, spaces, +, -, and parentheses only, 6-20 characters.");
        }

        var photo = string.IsNullOrWhiteSpace(request.PhotoDataUrl) ? null : request.PhotoDataUrl.Trim();
        if (photo is not null)
        {
            if (!photo.StartsWith("data:image/", StringComparison.Ordinal))
            {
                return BadRequest("Photo must be an image data URL.");
            }
            if (photo.Length > MaxPhotoDataUrlLength)
            {
                return BadRequest("Photo is too large — please use a smaller image.");
            }
        }

        user.DisplayName = displayName;
        user.PhoneNumber = phone;
        user.PhotoDataUrl = photo;

        var result = await userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            return BadRequest(result.Errors.Select(e => e.Description));
        }

        return Ok(new ProfileResponse(user.Id, user.Email!, user.DisplayName, user.PhoneNumber, user.PhotoDataUrl));
    }
}
