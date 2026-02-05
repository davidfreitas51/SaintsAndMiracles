using System.ComponentModel.DataAnnotations;
using Core.DTOs;
using Core.Interfaces.Services;
using Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RegistrationController(
    SignInManager<AppUser> signInManager,
    IAccountTokensService accountTokensService,
    IEmailSender<AppUser> emailSender) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
    {
        var tokenRecord = await accountTokensService.GetValidTokenAsync(registerDto.InviteToken);
        if (tokenRecord == null)
            return BadRequest(new ApiErrorResponse { Message = "Invalid or expired token" });

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
            return BadRequest(new ApiErrorResponse
            {
                Message = "User registration failed",
                Details = result.Errors.Select(e => e.Description)
            });
        }

        var roleResult = await signInManager.UserManager.AddToRoleAsync(user, tokenRecord.Role);
        if (!roleResult.Succeeded)
        {
            await signInManager.UserManager.DeleteAsync(user);

            return BadRequest(new ApiErrorResponse
            {
                Message = "Failed to assign role",
                Details = roleResult.Errors.Select(e => e.Description)
            });
        }

        await accountTokensService.ConsumeAsync(registerDto.InviteToken);

        var token = await signInManager.UserManager.GenerateEmailConfirmationTokenAsync(user);
        var confirmationLink = Url.Action(
            "ConfirmEmail",
            "Registration",
            new { userId = user.Id, token },
            Request.Scheme
        );

        await emailSender.SendConfirmationLinkAsync(user, user.Email, confirmationLink);

        return Created("", new { Message = "User created. Please check your email to confirm." });
    }

    [HttpGet("confirm-email")]
    public async Task<IActionResult> ConfirmEmail(
        [FromQuery][Required][MaxLength(100)] string userId,
        [FromQuery][Required][MaxLength(8000)] string token,
        [FromServices] IWebHostEnvironment env,
        [FromServices] IConfiguration config)
    {
        string frontUrl = env.IsDevelopment()
            ? "http://localhost:4200"
            : config["Frontend:BaseUrl"] ?? $"{Request.Scheme}://{Request.Host}";

        var user = await signInManager.UserManager.FindByIdAsync(userId);
        if (user == null)
            return Redirect($"{frontUrl}/account/email-confirmed?success=false");

        var result = await signInManager.UserManager.ConfirmEmailAsync(user, token);
        if (!result.Succeeded)
            return Redirect($"{frontUrl}/account/email-confirmed?success=false");

        return Redirect($"{frontUrl}/account/email-confirmed?success=true");
    }

    [HttpPost("resend-confirmation")]
    public async Task<IActionResult> ResendConfirmation([FromBody] ResendConfirmationDto resendConfirmation)
    {
        var user = await signInManager.UserManager.FindByEmailAsync(resendConfirmation.Email);
        if (user == null)
            return NotFound(new ApiErrorResponse { Message = "User not found" });

        if (await signInManager.UserManager.IsEmailConfirmedAsync(user))
            return BadRequest(new ApiErrorResponse { Message = "Email already confirmed" });

        var token = await signInManager.UserManager.GenerateEmailConfirmationTokenAsync(user);
        var confirmationLink = Url.Action(
            "ConfirmEmail",
            "Registration",
            new { userId = user.Id, token },
            Request.Scheme
        );

        await emailSender.SendConfirmationLinkAsync(user, user.Email, confirmationLink);

        return Ok(new { Message = "Confirmation email resent" });
    }

    [Authorize(Roles = "SuperAdmin")]
    [HttpPost("invite")]
    public async Task<IActionResult> GenerateInviteToken([FromBody] InviteRequest request)
    {
        var token = await accountTokensService.GenerateInviteAsync(request.Role);
        return Ok(token);
    }
}