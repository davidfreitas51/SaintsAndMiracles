using Core.DTOs;
using Core.Validation.Attributes;
using Xunit;

namespace Core.Tests.DTOs;

public class CurrentUserDtoTests
{
    [Fact]
    public void Should_Pass_When_AllFieldsAreValid()
    {
        var dto = new CurrentUserDto
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@test.com"
        };

        var results = ModelValidationHelper.Validate(dto);

        Assert.Empty(results);
    }

    [Fact]
    public void Should_Fail_When_FirstName_IsMissing()
    {
        var dto = new CurrentUserDto
        {
            FirstName = "",
            LastName = "Doe",
            Email = "john.doe@test.com"
        };

        var results = ModelValidationHelper.Validate(dto);

        Assert.Contains(
            results,
            r => r.ErrorMessage!.Contains(nameof(CurrentUserDto.FirstName))
        );
    }

    [Fact]
    public void Should_Fail_When_LastName_IsMissing()
    {
        var dto = new CurrentUserDto
        {
            FirstName = "John",
            LastName = "",
            Email = "john.doe@test.com"
        };

        var results = ModelValidationHelper.Validate(dto);

        Assert.Contains(
            results,
            r => r.ErrorMessage!.Contains(nameof(CurrentUserDto.LastName))
        );
    }

    [Fact]
    public void Should_Fail_When_Email_IsMissing()
    {
        var dto = new CurrentUserDto
        {
            FirstName = "John",
            LastName = "Doe",
            Email = ""
        };

        var results = ModelValidationHelper.Validate(dto);

        Assert.Contains(
            results,
            r => r.ErrorMessage!.Contains(nameof(CurrentUserDto.Email))
        );
    }

    [Fact]
    public void Should_Fail_When_Email_IsInvalid()
    {
        var dto = new CurrentUserDto
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "not-an-email"
        };

        var results = ModelValidationHelper.Validate(dto);

        Assert.Contains(
            results,
            r => r.MemberNames.Contains(nameof(CurrentUserDto.Email))
        );
    }

    [Fact]
    public void SafeEmail_Should_Fail_When_Unsafe_Characters_Are_Present()
    {
        var dto = new SafeEmailOnlyDto
        {
            Email = "john@test.com&evil"
        };

        var results = ModelValidationHelper.Validate(dto);

        Assert.Contains(
            results,
            r => r.MemberNames.Contains(nameof(SafeEmailOnlyDto.Email))
        );
    }


    private class SafeEmailOnlyDto
    {
        [SafeEmail]
        public string Email { get; set; } = string.Empty;
    }

}
