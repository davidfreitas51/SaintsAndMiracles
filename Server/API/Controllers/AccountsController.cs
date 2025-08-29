using System.Net;
using System.Security.Claims;
using Core.DTOs;
using Core.Interfaces.Services;
using Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

[Route("api/[controller]")]
[ApiController]

public class AccountsController(SignInManager<AppUser> signInManager, IAccountTokensService accountTokensService, IEmailSender<AppUser> emailSender) : ControllerBase
{
    [HttpPost("Register")]
    public async Task<IActionResult> Register(RegisterDto registerDto)
    {
        if (!await accountTokensService.ValidateAsync(registerDto.InviteToken))
        {
            return BadRequest(new ApiErrorResponse
            {
                Message = "Invalid or expired token"
            });
        }

        var existingUser = await signInManager.UserManager.FindByEmailAsync(registerDto.Email);
        if (existingUser != null)
        {
            return BadRequest(new ApiErrorResponse
            {
                Message = "Email is already registered"
            });
        }

        var user = new AppUser
        {
            FirstName = registerDto.FirstName,
            LastName = registerDto.LastName,
            Email = registerDto.Email,
            UserName = registerDto.Email
        };

        var result = await signInManager.UserManager.CreateAsync(user, registerDto.Password);
        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description);
            return BadRequest(new ApiErrorResponse
            {
                Message = "User registration failed",
                Details = errors
            });
        }

        var roleResult = await signInManager.UserManager.AddToRoleAsync(user, "Admin");
        if (!roleResult.Succeeded)
        {
            var errors = roleResult.Errors.Select(e => e.Description);
            return BadRequest(new ApiErrorResponse
            {
                Message = "Failed to assign role",
                Details = errors
            });
        }

        await accountTokensService.ConsumeAsync(registerDto.InviteToken);

        var token = await signInManager.UserManager.GenerateEmailConfirmationTokenAsync(user);

        var confirmationLink = Url.Action(
            "ConfirmEmail",
            "Accounts",
            new { userId = user.Id, token },
            Request.Scheme
        );

        await emailSender.SendConfirmationLinkAsync(user, user.Email, confirmationLink);

        return Created("", new { Message = "User created. Please check your email to confirm." });
    }


    [HttpGet("ConfirmEmail")]
    public async Task<IActionResult> ConfirmEmail(string userId, string token)
    {
        var user = await signInManager.UserManager.FindByIdAsync(userId);
        if (user == null)
            return Redirect("http://localhost:4200/account/email-confirmed?success=false");

        var result = await signInManager.UserManager.ConfirmEmailAsync(user, token);

        if (!result.Succeeded)
            return Redirect("http://localhost:4200/account/email-confirmed?success=false");

        return Redirect("http://localhost:4200/account/email-confirmed?success=true");
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        await signInManager.SignOutAsync();
        return NoContent();
    }

    [HttpGet("user-info")]
    public async Task<IActionResult> GetUserInfo()
    {
        if (User.Identity?.IsAuthenticated is false) return NoContent();

        var user = await signInManager.UserManager.Users.FirstOrDefaultAsync(x => x.Email == User.FindFirstValue(ClaimTypes.Email));

        if (user == null) return Unauthorized();

        return Ok(new
        {
            user.FirstName,
            user.LastName,
            user.Email
        });
    }

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
                "Accounts",
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

    [HttpPost("Invite")]
    [Authorize]
    public async Task<IActionResult> GenerateInviteToken()
    {
        var token = await accountTokensService.GenerateInviteAsync();
        return Ok(token);
    }

    [HttpPost("Resend-confirmation")]
    public async Task<IActionResult> ResendConfirmation([FromBody] ResendConfirmationDto resendConfirmation)
    {
        var user = await signInManager.UserManager.FindByEmailAsync(resendConfirmation.Email);
        if (user == null)
            return NotFound(new ApiErrorResponse { Message = "User not found" });

        if (await signInManager.UserManager.IsEmailConfirmedAsync(user))
            return BadRequest(new ApiErrorResponse { Message = "Email already confirmed" });

        var token = await signInManager.UserManager.GenerateEmailConfirmationTokenAsync(user);
        var encodedToken = WebUtility.UrlEncode(token);

        var confirmationLink = Url.Action(
            "ConfirmEmail",
            "Accounts",
            new { userId = user.Id, token = encodedToken },
            Request.Scheme
        );

        await emailSender.SendConfirmationLinkAsync(user, user.Email, confirmationLink);

        return Ok(new { Message = "Confirmation email resent" });
    }

    [Authorize]
    [HttpGet("current-user")]
    public ActionResult<CurrentUserDto> GetCurrentUser()
    {
        return new CurrentUserDto
        {
            FirstName = User.FindFirst("firstName")?.Value ?? "",
            LastName = User.FindFirst("lastName")?.Value ?? "",
            Email = User.FindFirst(ClaimTypes.Email)?.Value ?? ""
        };
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto model)
    {
        var user = await signInManager.UserManager.FindByEmailAsync(model.Email);
        if (user == null || !(await signInManager.UserManager.IsEmailConfirmedAsync(user)))
        {
            // Don't reveal that the user does not exist or isn't confirmed
            return Ok(new { Message = "If the email exists, a reset link has been sent." });
        }

        var token = await signInManager.UserManager.GeneratePasswordResetTokenAsync(user);
        var encodedToken = WebUtility.UrlEncode(token);

        var resetLink = Url.Action(
            "ResetPassword",
            "Accounts",
            new { email = user.Email, token = encodedToken },
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
