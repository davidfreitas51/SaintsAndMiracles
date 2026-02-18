using Core.DTOs;
using Xunit;

namespace Core.Tests.DTOs;

/// <summary>
/// Tests for ChangePasswordDto validation.
/// Ensures both current and new passwords are required for password change operations.
/// </summary>
public class ChangePasswordDtoTests : DtoTestBase
{
    private static ChangePasswordDto CreateValidDto() => new()
    {
        CurrentPassword = "OldPassword123!",
        NewPassword = "NewStrongPassword456!"
    };

    // ==================== VALID DTO ====================

    [Fact]
    public void Should_Pass_ValidDto()
    {
        var dto = CreateValidDto();

        var results = Validate(dto);

        AssertValid(results);
    }

    [Fact]
    public void Should_Pass_WithSimplePasswords()
    {
        var dto = new ChangePasswordDto
        {
            CurrentPassword = "oldpass",
            NewPassword = "newpass"
        };

        var results = Validate(dto);

        AssertValid(results);
    }

    [Fact]
    public void Should_Pass_WithComplexPasswords()
    {
        var dto = new ChangePasswordDto
        {
            CurrentPassword = "C0mpl3x!P@ssw0rd#2024",
            NewPassword = "Ev3nM0r3!C0mpl3x#P@ss$2025"
        };

        var results = Validate(dto);

        AssertValid(results);
    }

    // ==================== CURRENT PASSWORD VALIDATION ====================

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Should_Fail_EmptyCurrentPassword(string emptyPassword)
    {
        var dto = CreateValidDto();
        dto.CurrentPassword = emptyPassword;

        var results = Validate(dto);

        AssertInvalidProperty(results, nameof(ChangePasswordDto.CurrentPassword));
    }

    // ==================== NEW PASSWORD VALIDATION ====================

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Should_Fail_EmptyNewPassword(string emptyPassword)
    {
        var dto = CreateValidDto();
        dto.NewPassword = emptyPassword;

        var results = Validate(dto);

        AssertInvalidProperty(results, nameof(ChangePasswordDto.NewPassword));
    }

    [Fact]
    public void Should_Pass_NewPasswordSameAsCurrentPassword()
    {
        // Note: This tests DTO validation only. Business logic should prevent same passwords.
        var dto = new ChangePasswordDto
        {
            CurrentPassword = "SamePassword123!",
            NewPassword = "SamePassword123!"
        };

        var results = Validate(dto);

        // From DTO perspective, both are provided so it passes
        AssertValid(results);
    }

    // ==================== MULTIPLE ERRORS ====================

    [Fact]
    public void Should_Fail_BothPasswordsMissing()
    {
        var dto = new ChangePasswordDto
        {
            CurrentPassword = "",
            NewPassword = ""
        };

        var results = Validate(dto);

        AssertInvalid(results);
        AssertErrorCount(results, 2);
    }

    [Fact]
    public void Should_Fail_BothPasswordsWhitespace()
    {
        var dto = new ChangePasswordDto
        {
            CurrentPassword = "   ",
            NewPassword = "   "
        };

        var results = Validate(dto);

        AssertInvalid(results);
        // Could be 0 or 2 errors depending on how whitespace is handled
    }

    // ==================== EDGE CASES ====================

    [Fact]
    public void Should_Pass_VeryLongPasswords()
    {
        var dto = new ChangePasswordDto
        {
            CurrentPassword = new string('A', 500),
            NewPassword = new string('B', 500)
        };

        var results = Validate(dto);

        AssertValid(results);
    }

    [Fact]
    public void Should_Pass_WithSpecialCharacters()
    {
        var dto = new ChangePasswordDto
        {
            CurrentPassword = "P@$$w0rd!#%&*(){}[]<>?/\\|",
            NewPassword = "N3w!P@$$w0rd-_+=.,;:'\"`~"
        };

        var results = Validate(dto);

        AssertValid(results);
    }

    [Fact]
    public void Should_Pass_WithUnicodeCharacters()
    {
        var dto = new ChangePasswordDto
        {
            CurrentPassword = "пароль123!",
            NewPassword = "密码456!"
        };

        var results = Validate(dto);

        AssertValid(results);
    }

    [Fact]
    public void Should_Pass_WithEmojis()
    {
        var dto = new ChangePasswordDto
        {
            CurrentPassword = "Pass🔒Word123",
            NewPassword = "N3w🔑Pass456"
        };

        var results = Validate(dto);

        AssertValid(results);
    }

    [Fact]
    public void Should_Pass_SingleCharacterPasswords()
    {
        var dto = new ChangePasswordDto
        {
            CurrentPassword = "a",
            NewPassword = "b"
        };

        var results = Validate(dto);

        // DTO validation only checks Required, not minimum length
        AssertValid(results);
    }
}
