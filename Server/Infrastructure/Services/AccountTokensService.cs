using Core.Interfaces.Services;
using Core.Models;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

public class AccountTokensService(DataContext context, ITokenService tokenService, ILogger<AccountTokensService> logger) : IAccountTokensService
{

    public async Task<string> GenerateInviteAsync(string role)
    {
        var clearToken = tokenService.GenerateClearToken();
        var hash = tokenService.HashTokenBase64(clearToken);

        var invite = new AccountToken
        {
            Hash = hash,
            Role = role,
            ExpiresAtUtc = DateTime.UtcNow.Add(TimeSpan.FromHours(24)),
            IsUsed = false
        };

        context.AccountTokens.Add(invite);
        await context.SaveChangesAsync();

        logger.LogInformation("Invite token generated for role: {Role}", role);
        return clearToken;
    }


    public async Task<bool> ValidateAsync(string providedToken)
    {
        var invites = await context.AccountTokens
            .Where(i => !i.IsUsed && i.ExpiresAtUtc > DateTime.UtcNow)
            .ToListAsync();

        var match = invites.FirstOrDefault(i =>
            tokenService.VerifyToken(providedToken, i.Hash));

        if (match == null)
        {
            logger.LogWarning("Token validation failed: Token not found or expired");
            return false;
        }

        logger.LogInformation("Token validated successfully");
        return true;
    }

    public async Task<bool> ConsumeAsync(string providedToken)
    {
        var invites = await context.AccountTokens
            .Where(i => !i.IsUsed && i.ExpiresAtUtc > DateTime.UtcNow)
            .ToListAsync();

        var match = invites.FirstOrDefault(i =>
            tokenService.VerifyToken(providedToken, i.Hash));

        if (match == null)
        {
            logger.LogWarning("Token consumption failed: Token not found, expired, or already used");
            return false;
        }

        match.IsUsed = true;
        await context.SaveChangesAsync();

        logger.LogInformation("Invite token consumed successfully. Role: {Role}", match.Role);
        return true;
    }

    public async Task<AccountToken?> GetValidTokenAsync(string providedToken)
    {
        var invites = await context.AccountTokens
            .Where(i => !i.IsUsed && i.ExpiresAtUtc > DateTime.UtcNow)
            .ToListAsync();

        var token = invites.FirstOrDefault(i => tokenService.VerifyToken(providedToken, i.Hash));

        if (token == null)
        {
            logger.LogWarning("GetValidToken: Token not found, expired, or invalid");
            return null;
        }

        logger.LogInformation("Valid token retrieved for role: {Role}", token.Role);
        return token;
    }
}
