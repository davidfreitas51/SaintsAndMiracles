namespace Core.Interfaces.Services;

public interface IInviteService
{
    Task<string> GenerateInviteAsync(TimeSpan? lifetime = null);
    Task<bool> ValidateAndConsumeAsync(string providedToken);
}
