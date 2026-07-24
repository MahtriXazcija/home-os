using System.Security.Claims;
using HomeOS.Application.Common;
using HomeOS.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace HomeOS.Api.Controllers;

public record RegisterRequest(string Email, string Password, string DisplayName);
public record LoginRequest(string Email, string Password);
public record AuthResponse(string Token, Guid UserId, string Email, string DisplayName);
public record ForgotPasswordRequest(string Email);
public record ResetPasswordRequest(string Email, string Token, string NewPassword);

[ApiController]
[Route("api/auth")]
public class AuthController(
    UserManager<AppUser> userManager,
    IJwtTokenService tokenService,
    IEmailSender emailSender,
    IFrontendLinkBuilder linkBuilder,
    ILogger<AuthController> logger) : ControllerBase
{
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password) || string.IsNullOrWhiteSpace(request.DisplayName))
        {
            return BadRequest("Email, password, and display name are all required.");
        }

        var user = new AppUser
        {
            UserName = request.Email,
            Email = request.Email,
            DisplayName = request.DisplayName.Trim()
        };

        var result = await userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            return BadRequest(result.Errors.Select(e => e.Description));
        }

        var token = tokenService.CreateToken(user);
        return Ok(new AuthResponse(token, user.Id, user.Email!, user.DisplayName));
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user is null || !await userManager.CheckPasswordAsync(user, request.Password))
        {
            return Unauthorized("Invalid email or password.");
        }

        var token = tokenService.CreateToken(user);
        return Ok(new AuthResponse(token, user.Id, user.Email!, user.DisplayName));
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<AuthResponse>> Me()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (userId is null)
        {
            return Unauthorized();
        }

        var user = await userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return Unauthorized();
        }

        return Ok(new AuthResponse(string.Empty, user.Id, user.Email!, user.DisplayName));
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordRequest request)
    {
        // Always respond the same way whether or not the email exists, so
        // this endpoint can't be used to probe which addresses are registered.
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user is not null)
        {
            var token = await userManager.GeneratePasswordResetTokenAsync(user);
            var link = linkBuilder.BuildPasswordResetLink(user.Email!, token);
            var html = $"""
                <p>Someone (hopefully you) asked to reset the Home OS password for this account.</p>
                <p><a href="{link}">Click here to choose a new password</a></p>
                <p style="color:#6e6e73; font-size:12px;">If you didn't request this, you can ignore this email.</p>
                """;

            var result = await emailSender.SendAsync(user.Email!, user.DisplayName, "Reset your Home OS password", html, HttpContext.RequestAborted);
            if (!result.Success)
            {
                logger.LogError("Password reset email to {Email} failed: {Error}", user.Email, result.Error);
            }
        }

        return Ok(new { message = "If that email is registered, a reset link is on its way." });
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(ResetPasswordRequest request)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user is null)
        {
            return BadRequest("This reset link is invalid or has expired.");
        }

        var result = await userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);
        if (!result.Succeeded)
        {
            return BadRequest(result.Errors.Select(e => e.Description));
        }

        return Ok(new { message = "Password updated — you can sign in now." });
    }
}
