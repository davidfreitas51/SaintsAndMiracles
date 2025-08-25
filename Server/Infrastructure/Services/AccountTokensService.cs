using Core.Interfaces.Services;
using Core.Models;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public class AccountTokensService(DataContext context, ITokenService tokenService) : IAccountTokensService
{

    public async Task<string> GenerateInviteAsync(TimeSpan? lifetime = null)
    {
        var clearToken = tokenService.GenerateClearToken();
        var hash = tokenService.HashTokenBase64(clearToken);

        var invite = new AccountToken
        {
            Hash = hash,
            ExpiresAtUtc = DateTime.UtcNow.Add(lifetime ?? TimeSpan.FromHours(24)),
            IsUsed = false
        };

        context.AccountTokens.Add(invite);
        await context.SaveChangesAsync();

        return clearToken; 
    }

    public async Task<bool> ValidateAsync(string providedToken)
    {
        var invites = await context.AccountTokens
            .Where(i => !i.IsUsed && i.ExpiresAtUtc > DateTime.UtcNow)
            .ToListAsync();

        var match = invites.FirstOrDefault(i =>
            tokenService.VerifyToken(providedToken, i.Hash));

        return match != null;
    }

    public async Task<bool> ConsumeAsync(string providedToken)
    {
        var invites = await context.AccountTokens
            .Where(i => !i.IsUsed && i.ExpiresAtUtc > DateTime.UtcNow)
            .ToListAsync();

        var match = invites.FirstOrDefault(i =>
            tokenService.VerifyToken(providedToken, i.Hash));

        if (match == null) return false;

        match.IsUsed = true;
        await context.SaveChangesAsync();
        return true;
    }

}
