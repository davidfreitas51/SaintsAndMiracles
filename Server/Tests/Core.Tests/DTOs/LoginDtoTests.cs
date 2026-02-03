using System.ComponentModel.DataAnnotations;
using Core.DTOs;
using Xunit;

namespace Core.Tests.DTOs;

public class LoginDtoTests
{
    [Fact]
    public void Should_Fail_When_Email_Is_Missing()
    {
        var dto = new LoginDto
        {
            Email = "",
            Password = "password123"
        };

        var results = ModelValidationHelper.Validate(dto);

        Assert.Contains(
            results,
            r => r.MemberNames.Contains(nameof(LoginDto.Email))
        );
    }

    [Fact]
    public void Should_Fail_When_Email_Is_Invalid()
    {
        var dto = new LoginDto
        {
            Email = "not-an-email",
            Password = "password123"
        };

        var results = ModelValidationHelper.Validate(dto);

        Assert.Contains(
            results,
            r => r.MemberNames.Contains(nameof(LoginDto.Email))
        );
    }

    [Fact]
    public void Should_Fail_When_Email_Is_Unsafe()
    {
        var dto = new LoginDto
        {
            Email = "john@test.com<script>",
            Password = "password123"
        };

        var results = ModelValidationHelper.Validate(dto);

        Assert.Contains(
            results,
            r => r.MemberNames.Contains(nameof(LoginDto.Email))
        );
    }

    [Fact]
    public void Should_Fail_When_Password_Is_Missing()
    {
        var dto = new LoginDto
        {
            Email = "john@test.com",
            Password = ""
        };

        var results = ModelValidationHelper.Validate(dto);

        Assert.Contains(
            results,
            r => r.MemberNames.Contains(nameof(LoginDto.Password))
        );
    }

    [Fact]
    public void Should_Fail_When_Email_And_Password_Are_Missing()
    {
        var dto = new LoginDto
        {
            Email = "",
            Password = ""
        };

        var results = ModelValidationHelper.Validate(dto);

        Assert.Equal(2, results.Count);
    }

    [Fact]
    public void Should_Pass_When_Dto_Is_Valid()
    {
        var dto = new LoginDto
        {
            Email = "john@test.com",
            Password = "password123",
            RememberMe = true
        };

        var results = ModelValidationHelper.Validate(dto);

        Assert.Empty(results);
    }
}
