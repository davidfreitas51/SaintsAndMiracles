using Core.DTOs;
using Xunit;

namespace Core.Tests.DTOs;

/// <summary>
/// Tests for ChangeEmailRequestDto validation.
/// Ensures new email addresses are valid and safe for user account changes.
/// </summary>
public class ChangeEmailRequestDtoTests : DtoTestBase
{
    private static ChangeEmailRequestDto CreateValidDto() => new()
    {
        NewEmail = "newemail@example.com"
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
    [InlineData("user.name@domain.com")]
    [InlineData("simple@test.co.uk")]
    [InlineData("with+tag@example.com")]
    public void Should_Pass_ValidEmails(string validEmail)
    {
        var dto = CreateValidDto();
        dto.NewEmail = validEmail;

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
        dto.NewEmail = invalidEmail!;

        var results = Validate(dto);

        AssertInvalidProperty(results, nameof(ChangeEmailRequestDto.NewEmail));
    }

    [Theory]
    [InlineData("not-an-email")]
    [InlineData("missing-domain@")]
    [InlineData("@no-local.com")]
    [InlineData("double@@domain.com")]
    public void Should_Fail_InvalidEmailFormat(string invalidEmail)
    {
        var dto = CreateValidDto();
        dto.NewEmail = invalidEmail;

        var results = Validate(dto);

        AssertInvalidProperty(results, nameof(ChangeEmailRequestDto.NewEmail));
    }

    [Theory]
    [InlineData("<script>@test.com")]
    [InlineData("user'@domain.com")]
    [InlineData("test&inject@example.com")]
    public void Should_Fail_UnsafeEmail(string unsafeEmail)
    {
        var dto = CreateValidDto();
        dto.NewEmail = unsafeEmail;

        var results = Validate(dto);

        AssertInvalidProperty(results, nameof(ChangeEmailRequestDto.NewEmail));
    }

    [Fact]
    public void Should_Fail_EmailTooLong()
    {
        var dto = CreateValidDto();
        var longLocalPart = new string('a', 250);
        dto.NewEmail = $"{longLocalPart}@domain.com";

        var results = Validate(dto);

        AssertInvalidProperty(results, nameof(ChangeEmailRequestDto.NewEmail));
    }

    // ==================== EDGE CASES ====================

    [Fact]
    public void Should_Pass_EmailWithSubdomains()
    {
        var dto = CreateValidDto();
        dto.NewEmail = "user@mail.example.co.uk";

        var results = Validate(dto);

        AssertValid(results);
    }

    [Fact]
    public void Should_Pass_MixedCaseEmail()
    {
        var dto = CreateValidDto();
        dto.NewEmail = "User.Name@Example.COM";

        var results = Validate(dto);

        AssertValid(results);
    }
}
