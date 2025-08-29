using Core.DTOs;

namespace Core.Interfaces.Services;

public interface IAuthenticationService
{
    Task<CurrentUserDto?> LoginAsync(LoginDto loginDto);
    Task LogoutAsync();
    Task<CurrentUserDto?> GetCurrentUserAsync(string userId);
}