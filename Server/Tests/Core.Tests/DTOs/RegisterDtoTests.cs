using Core.DTOs;
using Xunit;

namespace Core.Tests.DTOs;

/// <summary>
/// Tests for RegisterDto validation.
/// Ensures all registration fields are properly validated including password confirmation and invite token.
/// </summary>
public class RegisterDtoTests : DtoTestBase
{
    private static string FixtureSecret(int length, char fill)
        => new(fill, length);

    private static string FixtureToken(int length = 52)
        => new('T', length);

    private static RegisterDto CreateValidDto() => new()
    {
        FirstName = "John",
        LastName = "Doe",
        Email = "john.doe@example.com",
        Password = FixtureSecret(14, 'p'),
        ConfirmPassword = FixtureSecret(14, 'p'),
        InviteToken = FixtureToken() // 52 chars - valid token length
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
    public void Should_Pass_WithHyphenatedName()
    {
        var dto = CreateValidDto();
        dto.FirstName = "Mary-Jane";
        dto.LastName = "O'Brien";

        var results = Validate(dto);

        AssertValid(results);
    }

    [Fact]
    public void Should_Pass_WithUnicodeNames()
    {
        var dto = CreateValidDto();
        dto.FirstName = "José";
        dto.LastName = "García";

        var results = Validate(dto);

        AssertValid(results);
    }

    // ==================== FIRST NAME VALIDATION ====================

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Should_Fail_EmptyFirstName(string? invalidFirstName)
    {
        var dto = CreateValidDto();
        dto.FirstName = invalidFirstName!;

        var results = Validate(dto);

        AssertInvalidProperty(results, nameof(RegisterDto.FirstName));
    }

    [Theory]
    [InlineData("A")] // Too short (< 2 chars)
    [InlineData("John123")] // Has numbers
    [InlineData("John@Doe")] // Has special chars
    [InlineData("!@#$")]
    public void Should_Fail_InvalidFirstName(string invalidFirstName)
    {
        var dto = CreateValidDto();
        dto.FirstName = invalidFirstName;

        var results = Validate(dto);

        AssertInvalidProperty(results, nameof(RegisterDto.FirstName));
    }

    [Fact]
    public void Should_Fail_FirstNameTooLong()
    {
        var dto = CreateValidDto();
        dto.FirstName = new string('A', 101); // Over 100 chars

        var results = Validate(dto);

        AssertInvalidProperty(results, nameof(RegisterDto.FirstName));
    }

    // ==================== LAST NAME VALIDATION ====================

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Should_Fail_EmptyLastName(string? invalidLastName)
    {
        var dto = CreateValidDto();
        dto.LastName = invalidLastName!;

        var results = Validate(dto);

        AssertInvalidProperty(results, nameof(RegisterDto.LastName));
    }

    [Theory]
    [InlineData("D")] // Too short
    [InlineData("Doe123")] // Has numbers
    [InlineData("Doe<script>")] // XSS attempt
    public void Should_Fail_InvalidLastName(string invalidLastName)
    {
        var dto = CreateValidDto();
        dto.LastName = invalidLastName;

        var results = Validate(dto);

        AssertInvalidProperty(results, nameof(RegisterDto.LastName));
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

        AssertInvalidProperty(results, nameof(RegisterDto.Email));
    }

    [Theory]
    [InlineData("invalid-email")]
    [InlineData("bad@domain")]
    [InlineData("@nodomain.com")]
    [InlineData("noatsign.com")]
    public void Should_Fail_InvalidEmailFormat(string invalidEmail)
    {
        var dto = CreateValidDto();
        dto.Email = invalidEmail;

        var results = Validate(dto);

        AssertInvalidProperty(results, nameof(RegisterDto.Email));
    }

    [Theory]
    [InlineData("user<script>@test.com")]
    [InlineData("admin'@example.com")]
    [InlineData("test&inject@domain.com")]
    public void Should_Fail_UnsafeEmail(string unsafeEmail)
    {
        var dto = CreateValidDto();
        dto.Email = unsafeEmail;

        var results = Validate(dto);

        AssertInvalidProperty(results, nameof(RegisterDto.Email));
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

        AssertInvalidProperty(results, nameof(RegisterDto.Password));
    }

    [Fact]
    public void Should_Pass_StrongPassword()
    {
        var dto = CreateValidDto();
        dto.Password = string.Concat(FixtureSecret(5, 'v'), "!@#", FixtureSecret(6, 'w'));
        dto.ConfirmPassword = dto.Password;

        var results = Validate(dto);

        AssertValid(results);
    }

    // ==================== PASSWORD CONFIRMATION ====================

    [Fact]
    public void Should_Fail_PasswordMismatch()
    {
        var dto = CreateValidDto();
        dto.ConfirmPassword = FixtureSecret(12, 'd');

        var results = Validate(dto);

        AssertInvalidProperty(results, nameof(RegisterDto.ConfirmPassword), "must match");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Should_Fail_EmptyConfirmPassword(string? invalidConfirmPassword)
    {
        var dto = CreateValidDto();
        dto.ConfirmPassword = invalidConfirmPassword!;

        var results = Validate(dto);

        AssertInvalid(results);
        // Should fail either because it's required OR doesn't match
    }

    [Fact]
    public void Should_Fail_SubtleMismatch()
    {
        var dto = CreateValidDto();
        dto.Password = FixtureSecret(14, 'p');
        dto.ConfirmPassword = string.Concat(FixtureSecret(14, 'p'), " "); // Extra space

        var results = Validate(dto);

        // Exact match required
        if (dto.ConfirmPassword != dto.Password)
        {
            AssertInvalidProperty(results, nameof(RegisterDto.ConfirmPassword));
        }
    }

    // ==================== INVITE TOKEN VALIDATION ====================

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Should_Fail_EmptyInviteToken(string? invalidToken)
    {
        var dto = CreateValidDto();
        dto.InviteToken = invalidToken!;

        var results = Validate(dto);

        AssertInvalidProperty(results, nameof(RegisterDto.InviteToken));
    }

    [Theory]
    [InlineData("short")] // Too short (< 32 chars)
    [InlineData("bad token with spaces")]
    [InlineData("invalid!@#$token")]
    public void Should_Fail_InvalidTokenFormat(string invalidToken)
    {
        var dto = CreateValidDto();
        dto.InviteToken = invalidToken;

        var results = Validate(dto);

        AssertInvalidProperty(results, nameof(RegisterDto.InviteToken));
    }

    [Fact]
    public void Should_Pass_ValidTokenWithDashAndUnderscore()
    {
        var dto = CreateValidDto();
        dto.InviteToken = string.Concat("VALID-", new string('A', 8), "_", new string('B', 10), "-", new string('C', 10));

        var results = Validate(dto);

        // Token validator allows alphanumeric, dash, and underscore
        AssertValidProperty(results, nameof(RegisterDto.InviteToken));
    }

    [Fact]
    public void Should_Fail_TokenTooLong()
    {
        var dto = CreateValidDto();
        dto.InviteToken = new string('A', 129); // Over 128 chars

        var results = Validate(dto);

        AssertInvalidProperty(results, nameof(RegisterDto.InviteToken));
    }

    // ==================== MULTIPLE ERRORS ====================

    [Fact]
    public void Should_Fail_AllFieldsMissing()
    {
        var dto = new RegisterDto
        {
            FirstName = "",
            LastName = "",
            Email = "",
            Password = "",
            ConfirmPassword = "",
            InviteToken = ""
        };

        var results = Validate(dto);

        AssertInvalid(results);
        Assert.True(results.Count >= 5, "Should have at least 5 validation errors");
    }

    [Fact]
    public void Should_Fail_MultipleFieldsInvalid()
    {
        var dto = new RegisterDto
        {
            FirstName = "123", // Invalid format
            LastName = "!@#", // Invalid format
            Email = "not-an-email",
            Password = FixtureSecret(4, 'w'),
            ConfirmPassword = FixtureSecret(8, 'd'),
            InviteToken = FixtureSecret(3, 'b')
        };

        var results = Validate(dto);

        AssertInvalid(results);
        Assert.True(results.Count >= 4, "Should have multiple validation errors");
    }

    // ==================== EDGE CASES ====================

    [Fact]
    public void Should_Handle_ExactlyMinNameLength()
    {
        var dto = CreateValidDto();
        dto.FirstName = "Jo"; // Exactly 2 chars
        dto.LastName = "Wu"; // Exactly 2 chars

        var results = Validate(dto);

        AssertValidProperty(results, nameof(RegisterDto.FirstName));
        AssertValidProperty(results, nameof(RegisterDto.LastName));
    }

    [Fact]
    public void Should_Handle_ExactlyMaxNameLength()
    {
        var dto = CreateValidDto();
        dto.FirstName = new string('A', 100); // Exactly 100 chars
        dto.LastName = new string('B', 100); // Exactly 100 chars

        var results = Validate(dto);

        AssertValidProperty(results, nameof(RegisterDto.FirstName));
        AssertValidProperty(results, nameof(RegisterDto.LastName));
    }

    [Fact]
    public void Should_Handle_TokenWithMixedCase()
    {
        var dto = CreateValidDto();
        dto.InviteToken = string.Concat(new string('A', 10), new string('b', 10), new string('3', 10), new string('Z', 10));

        var results = Validate(dto);

        AssertValidProperty(results, nameof(RegisterDto.InviteToken));
    }
}
