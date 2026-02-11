using Core.DTOs;
using Xunit;

namespace Core.Tests.DTOs;

public class ForgotPasswordDtoTests
{
    [Fact]
    public void Should_Be_Valid_When_Email_Is_Valid_And_Safe()
    {
        var dto = new ForgotPasswordDto
        {
            Email = "user@test.com"
        };

        var results = ModelValidationHelper.Validate(dto);

        Assert.Empty(results);
    }

    [Fact]
    public void Should_Fail_When_Email_Is_Missing()
    {
        var dto = new ForgotPasswordDto
        {
            Email = ""
        };

        var results = ModelValidationHelper.Validate(dto);

        Assert.Contains(
            results,
            r => r.MemberNames.Contains(nameof(ForgotPasswordDto.Email))
        );
    }

    [Fact]
    public void Should_Fail_When_Email_Is_Invalid()
    {
        var dto = new ForgotPasswordDto
        {
            Email = "not-an-email"
        };

        var results = ModelValidationHelper.Validate(dto);

        Assert.Contains(
            results,
            r =>
                r.MemberNames.Contains(nameof(ForgotPasswordDto.Email)) &&
                r.ErrorMessage!.Contains("valid email", StringComparison.OrdinalIgnoreCase)
        );
    }

    [Fact]
    public void Should_Fail_When_Email_Is_Unsafe()
    {
        var dto = new ForgotPasswordDto
        {
            Email = "<script>@test.com"
        };

        var results = ModelValidationHelper.Validate(dto);

        Assert.Contains(
            results,
            r => r.MemberNames.Contains(nameof(ForgotPasswordDto.Email))
        );
    }
}
