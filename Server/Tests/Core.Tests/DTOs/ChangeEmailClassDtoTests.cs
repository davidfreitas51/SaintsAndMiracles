using Core.DTOs;
using Xunit;

namespace Core.Tests.DTOs;

public class ChangeEmailRequestDtoTests
{
    [Fact]
    public void Should_BeValid_WhenEmailIsCorrect()
    {
        var dto = new ChangeEmailRequestDto
        {
            NewEmail = "new@email.com"
        };

        var results = ModelValidationHelper.Validate(dto);

        Assert.Empty(results);
    }

    [Fact]
    public void Should_Fail_WhenEmailIsMissing()
    {
        var dto = new ChangeEmailRequestDto
        {
            NewEmail = null!
        };

        var results = ModelValidationHelper.Validate(dto);

        Assert.Contains(
            results,
            r => r.MemberNames.Contains(nameof(ChangeEmailRequestDto.NewEmail))
        );
    }

    [Fact]
    public void Should_Fail_WhenEmailIsInvalidFormat()
    {
        var dto = new ChangeEmailRequestDto
        {
            NewEmail = "not-an-email"
        };

        var results = ModelValidationHelper.Validate(dto);

        Assert.Contains(
            results,
            r => r.MemberNames.Contains(nameof(ChangeEmailRequestDto.NewEmail))
        );
    }

    [Fact]
    public void Should_Fail_WhenEmailIsUnsafe()
    {
        var dto = new ChangeEmailRequestDto
        {
            NewEmail = "<script>@test.com"
        };

        var results = ModelValidationHelper.Validate(dto);

        Assert.NotEmpty(results);
    }
}
