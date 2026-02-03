using Core.DTOs;
using Xunit;

namespace Core.Tests.DTOs;

public class RegisterDtoTests
{
    private static RegisterDto CreateValidDto() => new()
    {
        FirstName = "John",
        LastName = "Doe",
        Email = "john.doe@example.com",
        Password = "StrongPassword123!",
        ConfirmPassword = "StrongPassword123!",
        InviteToken = "VALIDTOKEN123VALIDTOKEN123VALIDTOKEN123VALIDTOKEN123"
    };

    [Fact]
    public void Should_Pass_When_Dto_Is_Valid()
    {
        var dto = CreateValidDto();

        var results = ModelValidationHelper.Validate(dto);

        Assert.Empty(results);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Should_Fail_When_FirstName_Is_Invalid(string? invalidValue)
    {
        var dto = CreateValidDto();
        dto.FirstName = invalidValue!;

        var results = ModelValidationHelper.Validate(dto);

        Assert.Contains(results, r => r.MemberNames.Contains(nameof(RegisterDto.FirstName)));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Should_Fail_When_LastName_Is_Invalid(string? invalidValue)
    {
        var dto = CreateValidDto();
        dto.LastName = invalidValue!;

        var results = ModelValidationHelper.Validate(dto);

        Assert.Contains(results, r => r.MemberNames.Contains(nameof(RegisterDto.LastName)));
    }

    [Theory]
    [InlineData("invalid-email")]
    [InlineData("bad@domain")]
    public void Should_Fail_When_Email_Is_Invalid(string invalidEmail)
    {
        var dto = CreateValidDto();
        dto.Email = invalidEmail;

        var results = ModelValidationHelper.Validate(dto);

        Assert.Contains(results, r => r.ErrorMessage!.Contains("Email"));
    }


    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Should_Fail_When_Password_Is_Invalid(string? invalidPassword)
    {
        var dto = CreateValidDto();
        dto.Password = invalidPassword!;

        var results = ModelValidationHelper.Validate(dto);

        Assert.Contains(results, r => r.MemberNames.Contains(nameof(RegisterDto.Password)));
    }

    [Fact]
    public void Should_Fail_When_ConfirmPassword_Does_Not_Match()
    {
        var dto = CreateValidDto();
        dto.ConfirmPassword = "DifferentPassword!";

        var results = ModelValidationHelper.Validate(dto);

        Assert.Contains(results, r => r.MemberNames.Contains(nameof(RegisterDto.ConfirmPassword)));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("bad token!")]
    public void Should_Fail_When_InviteToken_Is_Invalid(string? invalidToken)
    {
        var dto = CreateValidDto();
        dto.InviteToken = invalidToken!;

        var results = ModelValidationHelper.Validate(dto);

        Assert.Contains(results, r => r.MemberNames.Contains(nameof(RegisterDto.InviteToken)));
    }
}
