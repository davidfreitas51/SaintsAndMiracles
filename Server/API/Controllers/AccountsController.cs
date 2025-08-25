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
        if (!await accountTokensService.ValidateAndConsumeAsync(registerDto.InviteToken))
        {
            return BadRequest("Invalid or expired token");
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
            return BadRequest(result.Errors);

        var roleResult = await signInManager.UserManager.AddToRoleAsync(user, "Admin");
        if (!roleResult.Succeeded)
            return BadRequest(roleResult.Errors);

        var token = await signInManager.UserManager.GenerateEmailConfirmationTokenAsync(user);

        var confirmationLink = Url.Action(
            "ConfirmEmail",
            "Accounts",
            new { userId = user.Id, token },
            Request.Scheme);

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
                "Account",
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
    public async Task<IActionResult> GenerateInviteToken()
    {
        var token = await accountTokensService.GenerateInviteAsync();
        return Ok(token);
    }
}
