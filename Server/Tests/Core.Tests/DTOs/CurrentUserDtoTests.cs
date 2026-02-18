using Core.DTOs;
using Core.Validation.Attributes;
using Xunit;

namespace Core.Tests.DTOs;

/// <summary>
/// Tests for CurrentUserDto validation.
/// Ensures user profile data (names and email) are properly validated.
/// </summary>
public class CurrentUserDtoTests : DtoTestBase
{
    private static CurrentUserDto CreateValidDto() => new()
    {
        FirstName = "John",
        LastName = "Doe",
        Email = "john.doe@example.com"
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
    public void Should_Pass_WithUnicodeNames()
    {
        var dto = new CurrentUserDto
        {
            FirstName = "José",
            LastName = "García",
            Email = "jose@example.com"
        };

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

        AssertInvalidProperty(results, nameof(CurrentUserDto.FirstName));
    }

    [Theory]
    [InlineData("J")] // Too short
    [InlineData("John123")] // Numbers
    [InlineData("John@")]
    public void Should_Fail_InvalidFirstName(string invalidFirstName)
    {
        var dto = CreateValidDto();
        dto.FirstName = invalidFirstName;

        var results = Validate(dto);

        AssertInvalidProperty(results, nameof(CurrentUserDto.FirstName));
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

        AssertInvalidProperty(results, nameof(CurrentUserDto.LastName));
    }

    [Theory]
    [InlineData("D")] // Too short
    [InlineData("Doe123")] // Numbers
    [InlineData("Doe!")]
    public void Should_Fail_InvalidLastName(string invalidLastName)
    {
        var dto = CreateValidDto();
        dto.LastName = invalidLastName;

        var results = Validate(dto);

        AssertInvalidProperty(results, nameof(CurrentUserDto.LastName));
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

        AssertInvalidProperty(results, nameof(CurrentUserDto.Email));
    }

    [Theory]
    [InlineData("not-an-email")]
    [InlineData("bad@domain")]
    [InlineData("double@@test.com")]
    public void Should_Fail_InvalidEmailFormat(string invalidEmail)
    {
        var dto = CreateValidDto();
        dto.Email = invalidEmail;

        var results = Validate(dto);

        AssertInvalidProperty(results, nameof(CurrentUserDto.Email));
    }

    [Theory]
    [InlineData("john@test.com&evil")]
    [InlineData("<script>@test.com")]
    public void Should_Fail_UnsafeEmail(string unsafeEmail)
    {
        var dto = CreateValidDto();
        dto.Email = unsafeEmail;

        var results = Validate(dto);

        AssertInvalidProperty(results, nameof(CurrentUserDto.Email));
    }

    // ==================== MULTIPLE ERRORS ====================

    [Fact]
    public void Should_Fail_AllFieldsMissing()
    {
        var dto = new CurrentUserDto
        {
            FirstName = "",
            LastName = "",
            Email = ""
        };

        var results = Validate(dto);

        AssertInvalid(results);
        AssertErrorCount(results, 3);
    }

    // ==================== EDGE CASES ====================

    [Fact]
    public void Should_Pass_EmailWithSubdomains()
    {
        var dto = CreateValidDto();
        dto.Email = "john.doe@mail.example.co.uk";

        var results = Validate(dto);

        AssertValid(results);
    }

    [Fact]
    public void Should_Pass_NameWithHyphenApostrophe()
    {
        var dto = new CurrentUserDto
        {
            FirstName = "Mary-Jane",
            LastName = "O'Brien",
            Email = "mary.jane@example.com"
        };

        var results = Validate(dto);

        AssertValid(results);
    }
}
