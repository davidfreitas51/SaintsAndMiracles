using Core.DTOs;

namespace Core.Interfaces.Services;

public interface IRegistrationService
{
    Task<bool> RegisterAsync(RegisterDto registerDto);
    Task<bool> ConfirmEmailAsync(string userId, string token);
    Task ResendConfirmationEmailAsync(string email);
}
