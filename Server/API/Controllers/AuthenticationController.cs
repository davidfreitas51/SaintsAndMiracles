using Core.DTOs;
using Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthenticationController(SignInManager<AppUser> signInManager, IEmailSender<AppUser> emailSender) : ControllerBase
{
    [HttpPost("Login")]
    public async Task<IActionResult> Login(LoginDto loginDto)
    {
        var user = await signInManager.UserManager.FindByEmailAsync(loginDto.Email);

        if (user == null)
            return Unauthorized("Invalid email or password");

        if (!await signInManager.UserManager.IsEmailConfirmedAsync(user))
        {
            var token = await signInManager.UserManager.GenerateEmailConfirmationTokenAsync(user);

            var confirmationLink = Url.Action(
                "ConfirmEmail",
                "Registration",
                new { userId = user.Id, token },
                Request.Scheme);

            await emailSender.SendConfirmationLinkAsync(user, user.Email, confirmationLink);

            return Unauthorized("Email not confirmed. A new confirmation email has been sent.");
        }

        var result = await signInManager.PasswordSignInAsync(user, loginDto.Password, loginDto.RememberMe, lockoutOnFailure: false);

        if (!result.Succeeded)
            return Unauthorized("Invalid email or password");
        
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
        await signInManager.SignOutAsync();
        return NoContent();
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto model)
    {
        var user = await signInManager.UserManager.FindByEmailAsync(model.Email);
        if (user == null || !await signInManager.UserManager.IsEmailConfirmedAsync(user))
        {
            return Ok(new { Message = "If the email exists, a reset link has been sent." });
        }

        var token = await signInManager.UserManager.GeneratePasswordResetTokenAsync(user);

        var resetLink = Url.Action(
            "ResetPassword",
            "Accounts",
            new { email = user.Email, token = token },
            Request.Scheme);

        await emailSender.SendPasswordResetLinkAsync(user, user.Email, resetLink);

        return Ok(new { Message = "If the email exists, a reset link has been sent." });
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(ResetPasswordDto model)
    {
        var user = await signInManager.UserManager.FindByEmailAsync(model.Email);
        if (user == null) return BadRequest(new { Message = "Invalid request" });

        var result = await signInManager.UserManager.ResetPasswordAsync(user, model.Token, model.NewPassword);
        if (!result.Succeeded)
        {
            return BadRequest(new { Message = "Password reset failed", Errors = result.Errors.Select(e => e.Description) });
        }

        return Ok(new { Message = "Password has been reset successfully" });
    }
}
