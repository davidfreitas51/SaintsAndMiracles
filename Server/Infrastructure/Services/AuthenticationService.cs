using Core.DTOs;
using Core.Interfaces.Services;

namespace Infrastructure.Services;

public class AuthenticationService : IAuthenticationService
{
    public Task<CurrentUserDto?> GetCurrentUserAsync(string userId)
    {
        throw new NotImplementedException();
    }

    public Task<CurrentUserDto?> LoginAsync(LoginDto loginDto)
    {
        throw new NotImplementedException();
    }

    public Task LogoutAsync()
    {
        throw new NotImplementedException();
    }
}
