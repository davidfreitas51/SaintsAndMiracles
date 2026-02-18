using Core.DTOs;
using Xunit;

namespace Core.Tests.DTOs;

/// <summary>
/// Tests for ResendConfirmationDto validation.
/// Ensures email is valid and safe for confirmation resend requests.
/// </summary>
public class ResendConfirmationDtoTests : DtoTestBase
{
    private static ResendConfirmationDto CreateValidDto() => new()
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
    [InlineData("test+verification@example.com")]
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

        AssertInvalidProperty(results, nameof(ResendConfirmationDto.Email));
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

        AssertInvalidProperty(results, nameof(ResendConfirmationDto.Email));
    }

    [Theory]
    [InlineData("<script>@test.com")]
    [InlineData("user'@domain.com")]
    public void Should_Fail_UnsafeEmail(string unsafeEmail)
    {
        var dto = CreateValidDto();
        dto.Email = unsafeEmail;

        var results = Validate(dto);

        AssertInvalidProperty(results, nameof(ResendConfirmationDto.Email));
    }

    // ==================== EDGE CASES ====================

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
}
