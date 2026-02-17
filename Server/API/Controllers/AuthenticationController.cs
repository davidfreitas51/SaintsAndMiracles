using Core.DTOs;
using Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthenticationController(SignInManager<AppUser> signInManager, IEmailSender<AppUser> emailSender, ILogger<AuthenticationController> logger) : ControllerBase
{
    [HttpPost("Login")]
    public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
    {
        var maskedEmail = MaskEmail(loginDto.Email);
        var user = await signInManager.UserManager.FindByEmailAsync(loginDto.Email);
        if (user == null)
        {
            logger.LogWarning("Login failed: User not found. Email={MaskedEmail}", maskedEmail);
            return Unauthorized("Invalid email or password");
        }

        if (!await signInManager.UserManager.IsEmailConfirmedAsync(user))
        {
            var token = await signInManager.UserManager.GenerateEmailConfirmationTokenAsync(user);

            var confirmationLink = Url.Action(
                "ConfirmEmail",
                "Registration",
                new { userId = user.Id, token },
                Request.Scheme);

            await emailSender.SendConfirmationLinkAsync(user, user.Email, confirmationLink);

            logger.LogWarning("Login failed: Email not confirmed. Email={MaskedEmail}, UserId={UserId}", maskedEmail, user.Id);
            return Unauthorized("Email not confirmed. A new confirmation email has been sent.");
        }

        var result = await signInManager.PasswordSignInAsync(user, loginDto.Password, loginDto.RememberMe, lockoutOnFailure: false);

        if (!result.Succeeded)
        {
            logger.LogWarning("Login failed: Invalid password. Email={MaskedEmail}, UserId={UserId}", maskedEmail, user.Id);
            return Unauthorized("Invalid email or password");
        }

        logger.LogInformation("User logged in successfully. Email={MaskedEmail}, UserId={UserId}", maskedEmail, user.Id);
        return Ok(new
        {
            Message = "Login successful",
            user.FirstName,
            user.LastName,
            user.Email
        });
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        var user = await signInManager.UserManager.GetUserAsync(User);
        var maskedEmail = user != null ? MaskEmail(user.Email ?? "unknown") : "unknown";

        await signInManager.SignOutAsync();

        logger.LogInformation("User logged out successfully. Email={MaskedEmail}, UserId={UserId}", maskedEmail, user?.Id);
        return NoContent();
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto model)
    {
        var maskedEmail = MaskEmail(model.Email);
        var user = await signInManager.UserManager.FindByEmailAsync(model.Email);
        if (user == null || !await signInManager.UserManager.IsEmailConfirmedAsync(user))
        {
            logger.LogWarning("Forgot password request for non-existent or unconfirmed email. Email={MaskedEmail}", maskedEmail);
            return Ok(new { Message = "If the email exists, a reset link has been sent." });
        }

        var token = await signInManager.UserManager.GeneratePasswordResetTokenAsync(user);

        var resetLink = Url.Action(
            "ResetPassword",
            "Accounts",
            new { email = user.Email, token = token },
            Request.Scheme);

        await emailSender.SendPasswordResetLinkAsync(user, user.Email, resetLink);

        logger.LogInformation("Password reset email sent. Email={MaskedEmail}, UserId={UserId}", maskedEmail, user.Id);
        return Ok(new { Message = "If the email exists, a reset link has been sent." });
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto model)
    {
        var maskedEmail = MaskEmail(model.Email);
        var user = await signInManager.UserManager.FindByEmailAsync(model.Email);
        if (user == null)
        {
            logger.LogWarning("Password reset failed: User not found. Email={MaskedEmail}", maskedEmail);
            return BadRequest(new { Message = "Invalid request" });
        }

        var result = await signInManager.UserManager.ResetPasswordAsync(user, model.Token, model.NewPassword);
        if (!result.Succeeded)
        {
            logger.LogWarning("Password reset failed: Token validation or other errors. Email={MaskedEmail}, UserId={UserId}", maskedEmail, user.Id);
            return BadRequest(new { Message = "Password reset failed", Errors = result.Errors.Select(e => e.Description) });
        }

        logger.LogInformation("Password reset successfully. Email={MaskedEmail}, UserId={UserId}", maskedEmail, user.Id);
        return Ok(new { Message = "Password has been reset successfully" });
    }

    private static string MaskEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email) || !email.Contains("@"))
            return "***";

        var parts = email.Split('@');
        var localPart = parts[0];
        var domain = parts[1];

        var maskedLocal = localPart.Length <= 2
            ? new string('*', localPart.Length)
            : localPart[0] + new string('*', localPart.Length - 2) + localPart[^1];

        return $"{maskedLocal}@{domain}";
    }
}
