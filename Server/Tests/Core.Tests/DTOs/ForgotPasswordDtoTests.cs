using Core.DTOs;
using Xunit;

namespace Core.Tests.DTOs;

/// <summary>
/// Tests for ForgotPasswordDto validation.
/// Ensures email is valid and safe for password reset requests.
/// </summary>
public class ForgotPasswordDtoTests : DtoTestBase
{
    private static ForgotPasswordDto CreateValidDto() => new()
    {
        Email = "user@example.com"
    };

    // ==================== VALID DTO ====================

    [Fact]
    public void Should_Pass_ValidDto()
    {
        var dto = CreateValidDto();

        var results = Validate(dto);

        AssertValid(results);
    }

    [Theory]
    [InlineData("simple@test.com")]
    [InlineData("user.name@domain.co.uk")]
    [InlineData("test+tag@example.com")]
    public void Should_Pass_ValidEmails(string validEmail)
    {
        var dto = CreateValidDto();
        dto.Email = validEmail;

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

        AssertInvalidProperty(results, nameof(ForgotPasswordDto.Email));
    }

    [Theory]
    [InlineData("not-an-email")]
    [InlineData("missing-domain@")]
    [InlineData("@no-local.com")]
    [InlineData("no-at-sign.com")]
    [InlineData("double@@domain.com")]
    public void Should_Fail_InvalidEmailFormat(string invalidEmail)
    {
        var dto = CreateValidDto();
        dto.Email = invalidEmail;

        var results = Validate(dto);

        AssertInvalidProperty(results, nameof(ForgotPasswordDto.Email), "valid email");
    }

    [Theory]
    [InlineData("<script>@test.com")]
    [InlineData("user<script>@domain.com")]
    [InlineData("test'@example.com")]
    [InlineData("admin&user@test.com")]
    public void Should_Fail_UnsafeEmail(string unsafeEmail)
    {
        var dto = CreateValidDto();
        dto.Email = unsafeEmail;

        var results = Validate(dto);

        AssertInvalidProperty(results, nameof(ForgotPasswordDto.Email));
    }

    [Fact]
    public void Should_Fail_EmailWithSpaces()
    {
        var dto = CreateValidDto();
        dto.Email = "user name@test.com";

        var results = Validate(dto);

        AssertInvalidProperty(results, nameof(ForgotPasswordDto.Email));
    }

    [Fact]
    public void Should_Fail_EmailTooLong()
    {
        var dto = CreateValidDto();
        var longLocalPart = new string('a', 250);
        dto.Email = $"{longLocalPart}@domain.com"; // Over 254 chars total

        var results = Validate(dto);

        AssertInvalidProperty(results, nameof(ForgotPasswordDto.Email));
    }

    // ==================== EDGE CASES ====================

    [Fact]
    public void Should_Pass_EmailWithSubdomains()
    {
        var dto = CreateValidDto();
        dto.Email = "user@mail.example.co.uk";

        var results = Validate(dto);

        AssertValid(results);
    }

    [Fact]
    public void Should_Pass_EmailWithNumbers()
    {
        var dto = CreateValidDto();
        dto.Email = "user123@test456.com";

        var results = Validate(dto);

        AssertValid(results);
    }

    [Fact]
    public void Should_Pass_EmailWithHyphen()
    {
        var dto = CreateValidDto();
        dto.Email = "user-name@test-domain.com";

        var results = Validate(dto);

        AssertValid(results);
    }

    [Fact]
    public void Should_Pass_MixedCaseEmail()
    {
        var dto = CreateValidDto();
        dto.Email = "User.Name@Example.COM";

        var results = Validate(dto);

        AssertValid(results);
    }
}
