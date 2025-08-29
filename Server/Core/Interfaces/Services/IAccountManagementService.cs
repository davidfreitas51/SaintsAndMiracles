using Core.DTOs;

namespace Core.Interfaces.Services;

public interface IAccountManagementService
{
    Task ForgotPasswordAsync(ForgotPasswordDto forgotPasswordDto);
    Task<bool> ResetPasswordAsync(ResetPasswordDto resetPasswordDto);
    //Task<bool> ChangePasswordAsync(string userId, ChangePasswordDto changePasswordDto);
    //Task<CurrentUserDto?> UpdateProfileAsync(string userId, UpdateProfileDto updateProfileDto);
}
