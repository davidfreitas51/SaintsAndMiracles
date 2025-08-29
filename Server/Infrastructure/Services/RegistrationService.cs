using Core.DTOs;
using Core.Interfaces.Services;

namespace Infrastructure.Services;

public class RegistrationService : IRegistrationService
{
    public Task<bool> ConfirmEmailAsync(string userId, string token)
    {
        throw new NotImplementedException();
    }

    public Task<bool> RegisterAsync(RegisterDto registerDto)
    {
        throw new NotImplementedException();
    }

    public Task ResendConfirmationEmailAsync(string email)
    {
        throw new NotImplementedException();
    }
}
