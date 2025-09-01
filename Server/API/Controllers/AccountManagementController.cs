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
public class AccountManagementController(UserManager<AppUser> userManager, IEmailSender<AppUser> emailSender, IConfiguration configuration) : ControllerBase
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
}
