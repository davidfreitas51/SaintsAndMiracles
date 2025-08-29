using Core.DTOs;
using Core.Interfaces.Services;

namespace Infrastructure.Services;

public class AccountManagementService : IAccountManagementService
{
    public Task ForgotPasswordAsync(ForgotPasswordDto forgotPasswordDto)
    {
        throw new NotImplementedException();
    }

    public Task<bool> ResetPasswordAsync(ResetPasswordDto resetPasswordDto)
    {
        throw new NotImplementedException();
    }
}
