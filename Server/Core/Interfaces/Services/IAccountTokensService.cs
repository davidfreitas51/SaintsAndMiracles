namespace Core.Interfaces.Services;

public interface IAccountTokensService
{
    Task<string> GenerateInviteAsync(TimeSpan? lifetime = null);
    Task<bool> ValidateAndConsumeAsync(string providedToken);
}
