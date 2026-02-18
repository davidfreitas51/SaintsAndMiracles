using Core.DTOs;
using Xunit;

namespace Core.Tests.DTOs;

/// <summary>
/// Tests for LoginDto validation.
/// Ensures login credentials are properly validated.
/// </summary>
public class LoginDtoTests : DtoTestBase
{
    private static LoginDto CreateValidDto() => new()
    {
        Email = "john.doe@example.com",
        Password = "SecurePassword123!",
        RememberMe = false
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
    public void Should_Pass_WithRememberMe()
    {
        var dto = CreateValidDto();
        dto.RememberMe = true;

        var results = Validate(dto);

        AssertValid(results);
    }

    [Fact]
    public void Should_Pass_WithComplexEmail()
    {
        var dto = CreateValidDto();
        dto.Email = "user+tag@sub.domain.co.uk";

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

        AssertInvalidProperty(results, nameof(LoginDto.Email));
    }

    [Theory]
    [InlineData("not-an-email")]
    [InlineData("missing@domain")]
    [InlineData("@nodomain.com")]
    [InlineData("noatsign.com")]
    [InlineData("double@@domain.com")]
    public void Should_Fail_InvalidEmailFormat(string invalidEmail)
    {
        var dto = CreateValidDto();
        dto.Email = invalidEmail;

        var results = Validate(dto);

        AssertInvalidProperty(results, nameof(LoginDto.Email));
    }

    [Theory]
    [InlineData("john@test.com<script>")]
    [InlineData("test'@example.com")]
    [InlineData("user\"quote@test.com")]
    [InlineData("admin&injection@test.com")]
    [InlineData("john<doe@test.com")]
    public void Should_Fail_UnsafeEmail(string unsafeEmail)
    {
        var dto = CreateValidDto();
        dto.Email = unsafeEmail;

        var results = Validate(dto);

        AssertInvalidProperty(results, nameof(LoginDto.Email));
    }

    [Fact]
    public void Should_Fail_EmailWithSpaces()
    {
        var dto = CreateValidDto();
        dto.Email = "john doe@test.com";

        var results = Validate(dto);

        AssertInvalidProperty(results, nameof(LoginDto.Email));
    }

    [Fact]
    public void Should_Fail_EmailTooLong()
    {
        var dto = CreateValidDto();
        dto.Email = new string('a', 250) + "@example.com";

        var results = Validate(dto);

        AssertInvalidProperty(results, nameof(LoginDto.Email), "exceed");
    }

    // ==================== PASSWORD VALIDATION ====================

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Should_Fail_EmptyPassword(string? invalidPassword)
    {
        var dto = CreateValidDto();
        dto.Password = invalidPassword!;

        var results = Validate(dto);

        AssertInvalidProperty(results, nameof(LoginDto.Password));
    }

    [Fact]
    public void Should_Pass_AnyNonEmptyPassword()
    {
        // Login should accept any password format (validation happens on server)
        var dto = CreateValidDto();
        dto.Password = "x"; // Very short but non-empty

        var results = Validate(dto);

        // Should only fail if [Required] or custom validation, not format
        // Actual password validation happens server-side
        AssertValidProperty(results, nameof(LoginDto.Password));
    }

    // ==================== MULTIPLE ERRORS ====================

    [Fact]
    public void Should_Fail_BothFieldsMissing()
    {
        var dto = new LoginDto
        {
            Email = "",
            Password = ""
        };

        var results = Validate(dto);

        AssertInvalid(results);
        AssertInvalidProperty(results, nameof(LoginDto.Email));
        AssertInvalidProperty(results, nameof(LoginDto.Password));
        Assert.True(results.Count >= 2, "Should have at least 2 errors");
    }

    [Fact]
    public void Should_Fail_EmailInvalidPasswordMissing()
    {
        var dto = new LoginDto
        {
            Email = "not-valid-email",
            Password = ""
        };

        var results = Validate(dto);

        AssertInvalid(results);
        AssertInvalidProperty(results, nameof(LoginDto.Email));
        AssertInvalidProperty(results, nameof(LoginDto.Password));
    }

    // ==================== EDGE CASES ====================

    [Fact]
    public void Should_Handle_VeryLongPassword()
    {
        var dto = CreateValidDto();
        dto.Password = new string('x', 1000);

        var results = Validate(dto);

        // Long passwords should be accepted (hashing will handle them)
        AssertValidProperty(results, nameof(LoginDto.Password));
    }

    [Fact]
    public void Should_Handle_SpecialCharactersInPassword()
    {
        var dto = CreateValidDto();
        dto.Password = "P@$$w0rd!#$%^&*()_+-=[]{}|;:',.<>?/~`";

        var results = Validate(dto);

        AssertValidProperty(results, nameof(LoginDto.Password));
    }

    [Fact]
    public void Should_Handle_UnicodeInPassword()
    {
        var dto = CreateValidDto();
        dto.Password = "Pássw0rd日本語العربية🔒";

        var results = Validate(dto);

        AssertValidProperty(results, nameof(LoginDto.Password));
    }
}
