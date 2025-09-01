using System.Security.Claims;
using Core.DTOs;
using Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AccountManagementController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, IEmailSender<AppUser> emailSender, IConfiguration configuration) : ControllerBase
{
    private readonly string frontendBaseUrl = configuration["Frontend:BaseUrl"];

    [Authorize]
    [HttpGet("me")]
    public ActionResult Me()
    {
        return Ok();
    }


    [Authorize]
    [HttpGet("current-user")]
    public async Task<ActionResult<CurrentUserDto>> GetCurrentUser()
    {
        var email = User.FindFirstValue(ClaimTypes.Email);
        if (email == null)
            return Unauthorized();

        var user = await userManager.Users
            .Where(u => u.Email == email)
            .Select(u => new CurrentUserDto
            {
                FirstName = u.FirstName,
                LastName = u.LastName,
                Email = u.Email
            })
            .FirstOrDefaultAsync();

        if (user == null)
            return Unauthorized();

        return Ok(user);
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
    {
        var user = await userManager.FindByEmailAsync(dto.Email);
        if (user == null) return Ok();

        var token = await userManager.GeneratePasswordResetTokenAsync(user);

        var resetLink = $"{frontendBaseUrl}/account/reset-password?email={Uri.EscapeDataString(user.Email)}&token={Uri.EscapeDataString(token)}";

        await emailSender.SendPasswordResetLinkAsync(user, user.Email, resetLink);

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
    [HttpPut("update-profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
    {
        var email = User.FindFirstValue(ClaimTypes.Email);
        if (email == null)
            return Unauthorized();

        var user = await userManager.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null)
            return Unauthorized();

        user.FirstName = dto.FirstName;
        user.LastName = dto.LastName;

        var result = await userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description).ToArray();
            return BadRequest(new { Message = "Profile update failed", Errors = errors });
        }

        return Ok(new
        {
            user.FirstName,
            user.LastName,
            user.Email
        });
    }

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

        if (user.Email == dto.NewEmail)
            return BadRequest(new { Message = "New email must be different from current email." });

        var existingUser = await userManager.FindByEmailAsync(dto.NewEmail);
        if (existingUser != null)
            return BadRequest(new { Message = "This email is already registered by another user." });

        var token = await userManager.GenerateChangeEmailTokenAsync(user, dto.NewEmail);

        var confirmationLink = Url.Action(
            "ConfirmEmailChange",
            "AccountManagement",
            new { userId = user.Id, email = dto.NewEmail, token },
            Request.Scheme
        );

        await emailSender.SendConfirmationLinkAsync(user, dto.NewEmail, confirmationLink);

        return Ok(new { Message = "Confirmation email sent to new address." });
    }

    [Authorize]
    [HttpGet("confirm-email-change")]
    public async Task<IActionResult> ConfirmEmailChange(string userId, string email, string token)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user == null)
            return Redirect($"{frontendBaseUrl}/account/email-confirmed?success=false");

        var result = await userManager.ChangeEmailAsync(user, email, token);
        if (!result.Succeeded)
            return Redirect($"{frontendBaseUrl}/account/email-confirmed?success=false");

        user.UserName = email;
        await userManager.UpdateAsync(user);

        await signInManager.SignOutAsync();

        return Redirect($"{frontendBaseUrl}/account/email-confirmed?success=true&logout=true");
    }

    [Authorize]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
            return Unauthorized();

        var user = await userManager.FindByIdAsync(userId);
        if (user == null)
            return Unauthorized();

        var result = await userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);

        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description).ToArray();
            return BadRequest(new { Message = "Password change failed", Errors = errors });
        }

        return Ok(new { Message = "Password changed successfully" });
    }
}
