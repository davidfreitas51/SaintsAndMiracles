using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Core.DTOs;
using Core.Models;
using Core.Validation.Attributes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AccountManagementController(
    UserManager<AppUser> userManager,
    SignInManager<AppUser> signInManager,
    IEmailSender<AppUser> emailSender,
    IConfiguration configuration,
    ILogger<AccountManagementController> logger
) : ControllerBase
{
    private readonly string frontendBaseUrl = configuration["Frontend:BaseUrl"]!;

    [Authorize]
    [HttpGet("me")]
    public IActionResult Me()
    {
        return Ok();
    }

    // ===================== USERS =====================

    [Authorize(Roles = "SuperAdmin")]
    [HttpGet("users")]
    public async Task<ActionResult<IEnumerable<object>>> GetUsers()
    {
        var users = userManager.Users.ToList();

        var result = new List<object>();

        foreach (var user in users)
        {
            var roles = await userManager.GetRolesAsync(user);

            result.Add(new
            {
                user.FirstName,
                user.LastName,
                user.Email,
                Roles = roles
            });
        }

        return Ok(result);
    }


    [Authorize(Roles = "SuperAdmin")]
    [HttpDelete("users/{email}")]
    public async Task<IActionResult> DeleteUserByEmail(
        [FromRoute, Required, SafeEmail] string email)
    {
        var adminUser = await userManager.GetUserAsync(User);
        var maskedEmail = MaskEmail(email);
        logger.LogInformation("Attempting to delete user. Email={MaskedEmail}, AdminUserId={AdminUserId}", maskedEmail, adminUser?.Id);

        var user = await userManager.FindByEmailAsync(email);
        if (user == null)
        {
            logger.LogWarning("User deletion failed: User not found. Email={MaskedEmail}, AdminUserId={AdminUserId}", maskedEmail, adminUser?.Id);
            return NotFound(new { message = "User not found." });
        }

        var currentUser = await userManager.GetUserAsync(User);
        if (currentUser != null && currentUser.Email == email)
        {
            logger.LogWarning("User deletion failed: Admin attempted to delete own account. Email={MaskedEmail}, AdminUserId={AdminUserId}", maskedEmail, adminUser?.Id);
            return BadRequest(new { message = "You cannot delete your own account." });
        }

        var result = await userManager.DeleteAsync(user);
        if (!result.Succeeded)
        {
            logger.LogWarning("User deletion failed: Database operation error. Email={MaskedEmail}, AdminUserId={AdminUserId}", maskedEmail, adminUser?.Id);
            return BadRequest(result.Errors);
        }

        logger.LogInformation("User deleted successfully. Email={MaskedEmail}, UserId={UserId}, AdminUserId={AdminUserId}", maskedEmail, user.Id, adminUser?.Id);
        return NoContent();
    }

    // ===================== CURRENT USER =====================

    [Authorize]
    [HttpGet("current-user")]
    public async Task<ActionResult<CurrentUserDto>> GetCurrentUser()
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null)
            return Unauthorized();

        return Ok(new CurrentUserDto
        {
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email
        });
    }

    [Authorize]
    [HttpPut("update-profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null)
            return Unauthorized();

        logger.LogInformation("Updating user profile. UserId={UserId}, Email={MaskedEmail}", user.Id, MaskEmail(user.Email ?? ""));

        user.FirstName = dto.FirstName;
        user.LastName = dto.LastName;

        var result = await userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            logger.LogWarning("Profile update failed. UserId={UserId}, Email={MaskedEmail}", user.Id, MaskEmail(user.Email ?? ""));
            var errors = result.Errors.Select(e => e.Description).ToArray();
            return BadRequest(new { Message = "Profile update failed", Errors = errors });
        }

        logger.LogInformation("User profile updated successfully. UserId={UserId}, FirstName={FirstName}, LastName={LastName}", user.Id, dto.FirstName, dto.LastName);
        return Ok(new
        {
            user.FirstName,
            user.LastName,
            user.Email
        });
    }

    // ===================== ROLES =====================

    [Authorize]
    [HttpGet("user-role")]
    public async Task<ActionResult<object>> GetCurrentRole()
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null)
            return Unauthorized();

        var roles = await userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault();

        if (role == null)
            return NotFound("Role not found for user");

        return Ok(new { role });
    }

    // ===================== PASSWORD & EMAIL =====================

    [Authorize]
    [HttpPost("request-email-change")]
    public async Task<IActionResult> RequestEmailChange([FromBody] ChangeEmailRequestDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
            return Unauthorized();

        var user = await userManager.FindByIdAsync(userId);
        if (user == null)
            return Unauthorized();

        var maskedOldEmail = MaskEmail(user.Email ?? "");
        var maskedNewEmail = MaskEmail(dto.NewEmail);
        logger.LogInformation("Email change requested. UserId={UserId}, OldEmail={OldEmail}, NewEmail={NewEmail}", userId, maskedOldEmail, maskedNewEmail);

        if (user.Email == dto.NewEmail)
        {
            logger.LogWarning("Email change request failed: New email same as current. UserId={UserId}, Email={MaskedEmail}", userId, maskedOldEmail);
            return BadRequest(new { Message = "New email must be different from current email." });
        }

        var token = await userManager.GenerateChangeEmailTokenAsync(user, dto.NewEmail);

        var confirmationLink = Url.Action(
            "ConfirmEmailChange",
            "AccountManagement",
            new { userId = user.Id, email = dto.NewEmail, token },
            Request.Scheme
        );

        await emailSender.SendConfirmationLinkAsync(user, dto.NewEmail, confirmationLink);

        logger.LogInformation("Email change confirmation sent. UserId={UserId}, NewEmail={NewEmail}", userId, maskedNewEmail);
        return Ok(new { Message = "Confirmation email sent to new address." });
    }


    [HttpGet("confirm-email-change")]
    public async Task<IActionResult> ConfirmEmailChange(
        [FromQuery][Required][MaxLength(100)] string userId,
        [FromQuery][Required][EmailAddress][MaxLength(256)] string email,
        [FromQuery][Required][MaxLength(8000)] string token)
    {
        var failUrl = $"{frontendBaseUrl}/account/email-confirmed?success=false";

        email = email.Trim();
        token = Uri.UnescapeDataString(token);

        var maskedEmail = MaskEmail(email);
        logger.LogInformation("Confirming email change. UserId={UserId}, NewEmail={NewEmail}", userId, maskedEmail);

        var user = await userManager.FindByIdAsync(userId);
        if (user == null)
        {
            logger.LogWarning("Email change confirmation failed: User not found. UserId={UserId}, NewEmail={NewEmail}", userId, maskedEmail);
            return Redirect(failUrl);
        }

        var result = await userManager.ChangeEmailAsync(user, email, token);
        if (!result.Succeeded)
        {
            logger.LogWarning("Email change confirmation failed: Token validation error. UserId={UserId}, NewEmail={NewEmail}", userId, maskedEmail);
            return Redirect(failUrl);
        }

        user.UserName = email;
        await userManager.UpdateAsync(user);

        await signInManager.SignOutAsync();

        logger.LogInformation("Email changed successfully. UserId={UserId}, NewEmail={NewEmail}", userId, maskedEmail);
        return Redirect($"{frontendBaseUrl}/account/email-confirmed?success=true&logout=true");
    }


    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
    {
        var user = await userManager.FindByEmailAsync(dto.Email);
        if (user == null)
            return Ok();

        var token = await userManager.GeneratePasswordResetTokenAsync(user);

        var resetLink =
            $"{frontendBaseUrl}/account/reset-password?email={Uri.EscapeDataString(user.Email!)}&token={Uri.EscapeDataString(token)}";

        await emailSender.SendPasswordResetLinkAsync(user, user.Email!, resetLink);

        return Ok();
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
    {
        var user = await userManager.FindByEmailAsync(dto.Email);
        if (user == null)
            return BadRequest(new { Message = "Invalid request" });

        var result = await userManager.ResetPasswordAsync(user, dto.Token, dto.NewPassword);
        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description).ToArray();
            return BadRequest(new { Message = "Password reset failed", Errors = errors });
        }

        return Ok();
    }

    [Authorize]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null)
            return Unauthorized();

        logger.LogInformation("Password change requested. UserId={UserId}, Email={MaskedEmail}", user.Id, MaskEmail(user.Email ?? ""));

        var result = await userManager.ChangePasswordAsync(
            user,
            dto.CurrentPassword,
            dto.NewPassword
        );

        if (!result.Succeeded)
        {
            logger.LogWarning("Password change failed. UserId={UserId}, Email={MaskedEmail}", user.Id, MaskEmail(user.Email ?? ""));
            var errors = result.Errors.Select(e => e.Description).ToArray();
            return BadRequest(new { Message = "Password change failed", Errors = errors });
        }

        await signInManager.SignOutAsync();

        logger.LogInformation("Password changed successfully and user signed out. UserId={UserId}, Email={MaskedEmail}", user.Id, MaskEmail(user.Email ?? ""));
        return Ok(new
        {
            Message = "Password changed successfully. Please log in again."
        });
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
