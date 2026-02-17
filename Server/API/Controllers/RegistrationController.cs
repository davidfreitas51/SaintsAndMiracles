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
    IEmailSender<AppUser> emailSender,
    ILogger<RegistrationController> logger) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
    {
        var maskedEmail = MaskEmail(registerDto.Email);
        logger.LogInformation("User registration attempt. Email={MaskedEmail}, FirstName={FirstName}, LastName={LastName}", maskedEmail, registerDto.FirstName, registerDto.LastName);

        var tokenRecord = await accountTokensService.GetValidTokenAsync(registerDto.InviteToken);
        if (tokenRecord == null)
        {
            logger.LogWarning("Registration failed: Invalid or expired token. Email={MaskedEmail}", maskedEmail);
            return BadRequest(new ApiErrorResponse { Message = "Invalid or expired token" });
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
            logger.LogWarning("Registration failed: User creation error. Email={MaskedEmail}", maskedEmail);
            return BadRequest(new ApiErrorResponse
            {
                Message = "User registration failed",
                Details = result.Errors.Select(e => e.Description)
            });
        }

        var roleResult = await signInManager.UserManager.AddToRoleAsync(user, tokenRecord.Role);
        if (!roleResult.Succeeded)
        {
            logger.LogWarning("Registration failed: Role assignment error. Email={MaskedEmail}, UserId={UserId}, Role={Role}", maskedEmail, user.Id, tokenRecord.Role);
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

        logger.LogInformation("User registered successfully. Email={MaskedEmail}, UserId={UserId}, Role={Role}", maskedEmail, user.Id, tokenRecord.Role);
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
        logger.LogInformation("Invite token generated. Role={Role}", request.Role);
        var token = await accountTokensService.GenerateInviteAsync(request.Role);
        logger.LogInformation("Invite token created successfully. Role={Role}, Token={TokenValue}", request.Role, token);
        return Ok(token);
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