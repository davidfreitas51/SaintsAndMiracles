using Core.DTOs;
using Xunit;

namespace Core.Tests.DTOs;

/// <summary>
/// Tests for ChangePasswordDto validation.
/// Ensures both current and new passwords are required for password change operations.
/// </summary>
public class ChangePasswordDtoTests : DtoTestBase
{
    private static string FixtureSecret(int length, char fill)
        => new(fill, length);

    private static string FixtureSecretWithSymbols()
        => string.Concat(new string('x', 6), "!@#", new string('y', 6));

    private static ChangePasswordDto CreateValidDto() => new()
    {
        CurrentPassword = FixtureSecret(14, 'o'),
        NewPassword = FixtureSecret(14, 'n')
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
            CurrentPassword = FixtureSecret(7, 'o'),
            NewPassword = FixtureSecret(7, 'n')
        };

        var results = Validate(dto);

        AssertValid(results);
    }

    [Fact]
    public void Should_Pass_WithComplexPasswords()
    {
        var dto = new ChangePasswordDto
        {
            CurrentPassword = FixtureSecretWithSymbols(),
            NewPassword = string.Concat(FixtureSecret(5, 'z'), "$%^", FixtureSecret(5, 'q'))
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
            CurrentPassword = FixtureSecret(12, 's'),
            NewPassword = FixtureSecret(12, 's')
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
            CurrentPassword = string.Concat(new string('a', 6), "!#%&", new string('b', 6)),
            NewPassword = string.Concat(new string('c', 6), "-_+=", new string('d', 6))
        };

        var results = Validate(dto);

        AssertValid(results);
    }

    [Fact]
    public void Should_Pass_WithUnicodeCharacters()
    {
        var dto = new ChangePasswordDto
        {
            CurrentPassword = string.Concat(FixtureSecret(4, 'q'), "日本語"),
            NewPassword = string.Concat(FixtureSecret(4, 'r'), "العربية")
        };

        var results = Validate(dto);

        AssertValid(results);
    }

    [Fact]
    public void Should_Pass_WithEmojis()
    {
        var dto = new ChangePasswordDto
        {
            CurrentPassword = string.Concat(FixtureSecret(4, 'p'), "🔒", FixtureSecret(4, 'q')),
            NewPassword = string.Concat(FixtureSecret(4, 'r'), "🔑", FixtureSecret(4, 's'))
        };

        var results = Validate(dto);

        AssertValid(results);
    }

    [Fact]
    public void Should_Pass_SingleCharacterPasswords()
    {
        var dto = new ChangePasswordDto
        {
            CurrentPassword = FixtureSecret(1, 'a'),
            NewPassword = FixtureSecret(1, 'b')
        };

        var results = Validate(dto);

        // DTO validation only checks Required, not minimum length
        AssertValid(results);
    }
}
