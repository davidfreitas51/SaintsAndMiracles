using Core.DTOs;
using Xunit;

namespace Core.Tests.DTOs;

public class ChangePasswordDtoTests
{
    [Fact]
    public void Should_Pass_When_AllFieldsAreProvided()
    {
        var dto = new ChangePasswordDto
        {
            CurrentPassword = "OldPassword123!",
            NewPassword = "NewPassword123!"
        };

        var results = ModelValidationHelper.Validate(dto);

        Assert.Empty(results);
    }

    [Fact]
    public void Should_Fail_When_CurrentPassword_IsMissing()
    {
        var dto = new ChangePasswordDto
        {
            CurrentPassword = "",
            NewPassword = "NewPassword123!"
        };

        var results = ModelValidationHelper.Validate(dto);

        Assert.Contains(
            results,
            r => r.ErrorMessage!.Contains("CurrentPassword")
        );
    }

    [Fact]
    public void Should_Fail_When_NewPassword_IsMissing()
    {
        var dto = new ChangePasswordDto
        {
            CurrentPassword = "OldPassword123!",
            NewPassword = ""
        };

        var results = ModelValidationHelper.Validate(dto);

        Assert.Contains(
            results,
            r => r.ErrorMessage!.Contains("NewPassword")
        );
    }

    [Fact]
    public void Should_Fail_When_BothPasswords_AreMissing()
    {
        var dto = new ChangePasswordDto
        {
            CurrentPassword = "",
            NewPassword = ""
        };

        var results = ModelValidationHelper.Validate(dto);

        Assert.Equal(2, results.Count);
    }
}
