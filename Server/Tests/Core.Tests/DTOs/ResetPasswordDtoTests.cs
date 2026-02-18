using Core.DTOs;
using Xunit;

namespace Core.Tests.DTOs;

/// <summary>
/// Tests for ResetPasswordDto validation.
/// Ensures password reset requests have valid email, token, and new password.
/// </summary>
public class ResetPasswordDtoTests : DtoTestBase
{
    private static ResetPasswordDto CreateValidDto() => new()
    {
        Email = "user@example.com",
        Token = "valid-reset-token-123456789abcdefghij",
        NewPassword = "MyNewPassword123!"
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
    public void Should_Pass_WithComplexPassword()
    {
        var dto = CreateValidDto();
        dto.NewPassword = "C0mpl3x!P@ssw0rd#2024";

        var results = Validate(dto);

        AssertValid(results);
    }

    // ==================== EMAIL VALIDATION ====================

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Should_Fail_EmptyEmail(string? invalidEmail)
    {
        var dto = CreateValidDto();
        dto.Email = invalidEmail!;

        var results = Validate(dto);

        AssertInvalidProperty(results, nameof(ResetPasswordDto.Email));
    }

    [Theory]
    [InlineData("invalid-email")]
    [InlineData("bad@domain")]
    [InlineData("john@@example.com")]
    [InlineData("@no-local.com")]
    public void Should_Fail_InvalidEmailFormat(string invalidEmail)
    {
        var dto = CreateValidDto();
        dto.Email = invalidEmail;

        var results = Validate(dto);

        AssertInvalidProperty(results, nameof(ResetPasswordDto.Email));
    }

    [Theory]
    [InlineData("<script>@test.com")]
    [InlineData("admin'@example.com")]
    public void Should_Fail_UnsafeEmail(string unsafeEmail)
    {
        var dto = CreateValidDto();
        dto.Email = unsafeEmail;

        var results = Validate(dto);

        AssertInvalidProperty(results, nameof(ResetPasswordDto.Email));
    }

    // ==================== TOKEN VALIDATION ====================

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Should_Fail_EmptyToken(string? invalidToken)
    {
        var dto = CreateValidDto();
        dto.Token = invalidToken!;

        var results = Validate(dto);

        AssertInvalidProperty(results, nameof(ResetPasswordDto.Token));
    }

    [Theory]
    [InlineData("token<script>")]
    [InlineData("bad&amp;token")]
    public void Should_Fail_UnsafeToken(string unsafeToken)
    {
        var dto = CreateValidDto();
        dto.Token = unsafeToken;

        var results = Validate(dto);

        AssertInvalidProperty(results, nameof(ResetPasswordDto.Token));
    }

    [Fact]
    public void Should_Pass_AlphanumericToken()
    {
        var dto = CreateValidDto();
        dto.Token = "ABC123def456GHI789jkl012MNO345";

        var results = Validate(dto);

        AssertValidProperty(results, nameof(ResetPasswordDto.Token));
    }

    [Fact]
    public void Should_Pass_TokenWithHyphens()
    {
        var dto = CreateValidDto();
        dto.Token = "valid-token-with-hyphens-abc123";

        var results = Validate(dto);

        AssertValidProperty(results, nameof(ResetPasswordDto.Token));
    }

    // ==================== NEW PASSWORD VALIDATION ====================

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Should_Fail_EmptyNewPassword(string? invalidPassword)
    {
        var dto = CreateValidDto();
        dto.NewPassword = invalidPassword!;

        var results = Validate(dto);

        AssertInvalidProperty(results, nameof(ResetPasswordDto.NewPassword));
    }

    [Fact]
    public void Should_Pass_SimplePassword()
    {
        var dto = CreateValidDto();
        dto.NewPassword = "simple";

        var results = Validate(dto);

        // DTO validation only checks Required, not strength
        AssertValid(results);
    }

    [Fact]
    public void Should_Pass_VeryLongPassword()
    {
        var dto = CreateValidDto();
        dto.NewPassword = new string('A', 500);

        var results = Validate(dto);

        AssertValid(results);
    }

    [Fact]
    public void Should_Pass_PasswordWithSpecialCharacters()
    {
        var dto = CreateValidDto();
        dto.NewPassword = "P@$$w0rd!#%&*(){}[]<>?/\\|";

        var results = Validate(dto);

        AssertValid(results);
    }

    [Fact]
    public void Should_Pass_PasswordWithUnicode()
    {
        var dto = CreateValidDto();
        dto.NewPassword = "密码123!пароль";

        var results = Validate(dto);

        AssertValid(results);
    }

    // ==================== MULTIPLE ERRORS ====================

    [Fact]
    public void Should_Fail_AllFieldsMissing()
    {
        var dto = new ResetPasswordDto
        {
            Email = "",
            Token = "",
            NewPassword = ""
        };

        var results = Validate(dto);

        AssertInvalid(results);
        AssertErrorCount(results, 3);
    }

    [Fact]
    public void Should_Fail_MultipleInvalidFields()
    {
        var dto = new ResetPasswordDto
        {
            Email = "not-an-email",
            Token = "bad<token>",
            NewPassword = ""
        };

        var results = Validate(dto);

        AssertInvalid(results);
        Assert.True(results.Count >= 3, "Should have at least 3 validation errors");
    }

    // ==================== EDGE CASES ====================

    [Fact]
    public void Should_Pass_EmailWithSubdomains()
    {
        var dto = CreateValidDto();
        dto.Email = "user@mail.subdomain.example.com";

        var results = Validate(dto);

        AssertValid(results);
    }

    [Fact]
    public void Should_Pass_TokenWithNumbers()
    {
        var dto = CreateValidDto();
        dto.Token = "1234567890abcdefghijklmnopqrstuvwxyz";

        var results = Validate(dto);

        AssertValid(results);
    }

    [Fact]
    public void Should_Pass_MixedCaseToken()
    {
        var dto = CreateValidDto();
        dto.Token = "MiXeDCaSe-ToKeN-123";

        var results = Validate(dto);

        AssertValid(results);
    }
}
