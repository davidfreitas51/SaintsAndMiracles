using Core.DTOs;
using Xunit;

namespace Core.Tests.DTOs;

public class ResendConfirmationDtoTests
{
    private static ResendConfirmationDto CreateValidDto() => new()
    {
        Email = "valid@example.com"
    };

    [Fact]
    public void Should_Pass_When_Dto_Is_Valid()
    {
        // Arrange
        var dto = CreateValidDto();

        // Act
        var results = ModelValidationHelper.Validate(dto);

        // Assert
        Assert.Empty(results);
    }

    [Fact]
    public void Should_Fail_When_Email_Is_Null()
    {
        var dto = CreateValidDto();
        dto.Email = null!;

        var results = ModelValidationHelper.Validate(dto);

        Assert.Contains(results, r => r.MemberNames.Contains(nameof(ResendConfirmationDto.Email)));
    }

    [Fact]
    public void Should_Fail_When_Email_Is_Empty()
    {
        var dto = CreateValidDto();
        dto.Email = string.Empty;

        var results = ModelValidationHelper.Validate(dto);

        Assert.Contains(results, r => r.MemberNames.Contains(nameof(ResendConfirmationDto.Email)));
    }

    [Theory]
    [InlineData("invalid-email")]
    [InlineData("bad@domain")]
    [InlineData("john@@example.com")]
    public void Should_Fail_When_Email_Is_Invalid(string invalidEmail)
    {
        var dto = CreateValidDto();
        dto.Email = invalidEmail;

        var results = ModelValidationHelper.Validate(dto);

        Assert.Contains(results, r => r.MemberNames.Contains(nameof(ResendConfirmationDto.Email)));
    }
}
