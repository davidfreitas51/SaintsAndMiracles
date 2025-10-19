using Core.Models;

namespace Core.Interfaces.Services;

public interface IAccountTokensService
{
    Task<string> GenerateInviteAsync(string role);
    public Task<bool> ValidateAsync(string token);
    public Task<bool> ConsumeAsync(string token);
    public Task<AccountToken> GetValidTokenAsync(string providedToken);

}
