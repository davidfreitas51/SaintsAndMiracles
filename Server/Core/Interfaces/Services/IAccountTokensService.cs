namespace Core.Interfaces.Services;

public interface IAccountTokensService
{
    Task<string> GenerateInviteAsync(TimeSpan? lifetime = null);
    public Task<bool> ValidateAsync(string token);
    public Task<bool> ConsumeAsync(string token);

}
